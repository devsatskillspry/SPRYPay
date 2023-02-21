using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Abstractions.Custodians;
using SPRYPayServer.Abstractions.Extensions;
using SPRYPayServer.Abstractions.Models;
using SPRYPayServer.Abstractions.Services;
using SPRYPayServer.Client;
using SPRYPayServer.Common;
using SPRYPayServer.Configuration;
using SPRYPayServer.Controllers;
using SPRYPayServer.Controllers.Greenfield;
using SPRYPayServer.Data;
using SPRYPayServer.Data.Payouts.LightningLike;
using SPRYPayServer.Forms;
using SPRYPayServer.HostedServices;
using SPRYPayServer.Lightning;
using SPRYPayServer.Logging;
using SPRYPayServer.PaymentRequest;
using SPRYPayServer.Payments;
using SPRYPayServer.Payments.Bitcoin;
using SPRYPayServer.Payments.Lightning;
using SPRYPayServer.Payments.PayJoin;
using SPRYPayServer.PayoutProcessors;
using SPRYPayServer.Plugins;
using SPRYPayServer.Security;
using SPRYPayServer.Security.Bitpay;
using SPRYPayServer.Security.Greenfield;
using SPRYPayServer.Services;
using SPRYPayServer.Services.Apps;
using SPRYPayServer.Services.Custodian.Client;
using SPRYPayServer.Services.Fees;
using SPRYPayServer.Services.Invoices;
using SPRYPayServer.Services.Labels;
using SPRYPayServer.Services.Mails;
using SPRYPayServer.Services.Notifications;
using SPRYPayServer.Services.Notifications.Blobs;
using SPRYPayServer.Services.PaymentRequests;
using SPRYPayServer.Services.Rates;
using SPRYPayServer.Services.Stores;
using SPRYPayServer.Services.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;
using NBitpayClient;
using NBXplorer.DerivationStrategy;
using Newtonsoft.Json;
using NicolasDorier.RateLimits;
using Serilog;
using ExchangeSharp;
using SPRYPayServer.Rating;
using System.Configuration.Provider;
using SPRYPayServer.Rating.Providers;
#if ALTCOINS
using SPRYPayServer.Services.Altcoins.Monero;
using SPRYPayServer.Services.Altcoins.Zcash;
#endif
namespace SPRYPayServer.Hosting
{
    public static class SPRYPayServerServices
    {
        public static IServiceCollection RegisterJsonConverter(this IServiceCollection services, Func<SPRYPayNetwork, JsonConverter> create)
        {
            services.AddSingleton<IJsonConverterRegistration, JsonConverterRegistration>((s) => new JsonConverterRegistration(create));
            return services;
        }
        public static IServiceCollection AddSPRYPayServer(this IServiceCollection services, IConfiguration configuration, Logs logs)
        {
            services.AddSingleton<MvcNewtonsoftJsonOptions>(o => o.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value);
            services.AddSingleton<JsonSerializerSettings>(o => o.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value.SerializerSettings);
            services.AddDbContext<ApplicationDbContext>((provider, o) =>
            {
                var factory = provider.GetRequiredService<ApplicationDbContextFactory>();
                factory.ConfigureBuilder(o);
            });
            services.AddHttpClient();
            services.AddHttpClient(nameof(ExplorerClientProvider), httpClient =>
            {
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
            });
            services.AddHttpClient<PluginBuilderClient>((prov, httpClient) =>
            {
                var p = prov.GetRequiredService<PoliciesSettings>();
                var pluginSource = p.PluginSource ?? PoliciesSettings.DefaultPluginSource;
                if (pluginSource.EndsWith('/'))
                    pluginSource = pluginSource.Substring(0, pluginSource.Length - 1);
                if (!Uri.TryCreate(pluginSource, UriKind.Absolute, out var r) || (r.Scheme != "https" && r.Scheme != "http"))
                    r = new Uri(PoliciesSettings.DefaultPluginSource, UriKind.Absolute);
                httpClient.BaseAddress = r;
            });

            services.AddSingleton<Logs>(logs);
            services.AddSingleton<SPRYPayNetworkJsonSerializerSettings>();

            services.AddPayJoinServices();
#if ALTCOINS
            if (configuration.SupportChain("xmr"))
                services.AddMoneroLike();
            if (configuration.SupportChain("yec") || configuration.SupportChain("zec"))
                services.AddZcashLike();
#endif
            services.AddScoped<IScopeProvider, ScopeProvider>();
            services.TryAddSingleton<SettingsRepository>();
            services.TryAddSingleton<ISettingsRepository>(provider => provider.GetService<SettingsRepository>());
            services.TryAddSingleton<IStoreRepository>(provider => provider.GetService<StoreRepository>());
            services.TryAddSingleton<TorServices>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<TorServices>());
            services.AddSingleton<ISwaggerProvider, DefaultSwaggerProvider>();
            services.TryAddSingleton<SocketFactory>();
            services.TryAddSingleton<LightningClientFactoryService>();
            services.AddHttpClient(LightningClientFactoryService.OnionNamedClient)
                .ConfigurePrimaryHttpMessageHandler<Socks5HttpClientHandler>();
            services.TryAddSingleton<InvoicePaymentNotification>();
            services.TryAddSingleton<SPRYPayServerOptions>(o =>
                o.GetRequiredService<IOptions<SPRYPayServerOptions>>().Value);
            // Don't move this StartupTask, we depend on it being right here
            if (configuration["POSTGRES"] != null && (configuration["SQLITEFILE"] != null || configuration["MYSQL"] != null))
                services.AddStartupTask<ToPostgresMigrationStartupTask>();
            services.AddStartupTask<MigrationStartupTask>();

            //
            AddSettingsAccessor<PoliciesSettings>(services);
            AddSettingsAccessor<ThemeSettings>(services);
            //
            services.AddStartupTask<BlockExplorerLinkStartupTask>();
            services.TryAddSingleton<InvoiceRepository>();
            services.AddSingleton<PaymentService>();
            services.AddSingleton<SPRYPayServerEnvironment>();
            services.TryAddSingleton<TokenRepository>();
            services.TryAddSingleton<WalletRepository>();
            services.TryAddSingleton<EventAggregator>();
            services.TryAddSingleton<PaymentRequestService>();
            services.TryAddSingleton<UserService>();
            services.AddSingleton<CustodianAccountRepository>();
            services.TryAddSingleton<WalletHistogramService>();
            services.AddSingleton<ApplicationDbContextFactory>();
            services.AddOptions<SPRYPayServerOptions>().Configure(
                (options) =>
                {
                    options.LoadArgs(configuration, logs);
                });
            services.AddOptions<DataDirectories>().Configure(
                (options) =>
                {
                    options.Configure(configuration);
                });
            services.AddOptions<DatabaseOptions>().Configure<IOptions<DataDirectories>>(
                (options, datadirs) =>
                {
                    var postgresConnectionString = configuration["postgres"];
                    var mySQLConnectionString = configuration["mysql"];
                    var sqliteFileName = configuration["sqlitefile"];

                    if (!string.IsNullOrEmpty(postgresConnectionString))
                    {
                        options.DatabaseType = DatabaseType.Postgres;
                        options.ConnectionString = postgresConnectionString;
                    }
                    else if (!string.IsNullOrEmpty(mySQLConnectionString))
                    {
                        options.DatabaseType = DatabaseType.MySQL;
                        options.ConnectionString = mySQLConnectionString;
                    }
                    else if (!string.IsNullOrEmpty(sqliteFileName))
                    {
                        options.DatabaseType = DatabaseType.Sqlite;
                        options.ConnectionString = "Data Source=" + datadirs.Value.ToDatadirFullPath(sqliteFileName);
                    }
                    else
                    {
                        throw new InvalidOperationException("No database option was configured.");
                    }
                });
            services.AddOptions<NBXplorerOptions>().Configure<SPRYPayNetworkProvider>(
                (options, btcPayNetworkProvider) =>
                {
                    foreach (SPRYPayNetwork btcPayNetwork in btcPayNetworkProvider.GetAll().OfType<SPRYPayNetwork>())
                    {
                        NBXplorerConnectionSetting setting =
                            new NBXplorerConnectionSetting
                            {
                                CryptoCode = btcPayNetwork.CryptoCode,
                                ExplorerUri = configuration.GetOrDefault<Uri>(
                                    $"{btcPayNetwork.CryptoCode}.explorer.url",
                                    btcPayNetwork.NBXplorerNetwork.DefaultSettings.DefaultUrl),
                                CookieFile = configuration.GetOrDefault<string>(
                                    $"{btcPayNetwork.CryptoCode}.explorer.cookiefile",
                                    btcPayNetwork.NBXplorerNetwork.DefaultSettings.DefaultCookieFile)
                            };
                        options.NBXplorerConnectionSettings.Add(setting);
                        options.ConnectionString = configuration.GetOrDefault<string>("explorer.postgres", null);
                        var blockExplorerLink = configuration.GetOrDefault<string>("blockexplorerlink", null);
                        if (!string.IsNullOrEmpty(blockExplorerLink))
                        {
                            btcPayNetwork.BlockExplorerLink = blockExplorerLink;
                        }
                    }
                });
            services.AddOptions<LightningNetworkOptions>().Configure<SPRYPayNetworkProvider>(
                (options, btcPayNetworkProvider) =>
                {
                    foreach (var net in btcPayNetworkProvider.GetAll().OfType<SPRYPayNetwork>())
                    {
                        var lightning = configuration.GetOrDefault<string>($"{net.CryptoCode}.lightning", string.Empty);
                        if (lightning.Length != 0)
                        {
                            if (!LightningConnectionString.TryParse(lightning, true, out var connectionString,
                                out var error))
                            {
                                logs.Configuration.LogWarning($"Invalid setting {net.CryptoCode}.lightning, " +
                                                              Environment.NewLine +
                                                              $"If you have a c-lightning server use: 'type=clightning;server=/root/.lightning/lightning-rpc', " +
                                                              Environment.NewLine +
                                                              $"If you have a lightning charge server: 'type=charge;server=https://charge.example.com;api-token=yourapitoken'" +
                                                              Environment.NewLine +
                                                              $"If you have a lnd server: 'type=lnd-rest;server=https://lnd:lnd@lnd.example.com;macaroon=abf239...;certthumbprint=2abdf302...'" +
                                                              Environment.NewLine +
                                                              $"              lnd server: 'type=lnd-rest;server=https://lnd:lnd@lnd.example.com;macaroonfilepath=/root/.lnd/admin.macaroon;certthumbprint=2abdf302...'" +
                                                              Environment.NewLine +
                                                              $"If you have an eclair server: 'type=eclair;server=http://eclair.com:4570;password=eclairpassword;bitcoin-host=bitcoind:37393;bitcoin-auth=bitcoinrpcuser:bitcoinrpcpassword" +
                                                              Environment.NewLine +
                                                              $"               eclair server: 'type=eclair;server=http://eclair.com:4570;password=eclairpassword;bitcoin-host=bitcoind:37393" +
                                                              Environment.NewLine +
                                                              $"Error: {error}" + Environment.NewLine +
                                                              "This service will not be exposed through SPRYPay Server");
                            }
                            else
                            {
                                if (connectionString.IsLegacy)
                                {
                                    logs.Configuration.LogWarning(
                                        $"Setting {net.CryptoCode}.lightning is a deprecated format, it will work now, but please replace it for future versions with '{connectionString.ToString()}'");
                                }
                                options.InternalLightningByCryptoCode.Add(net.CryptoCode, connectionString);
                            }
                        }
                    }
                });
            services.AddOptions<ExternalServicesOptions>().Configure<SPRYPayNetworkProvider>(
                (options, btcPayNetworkProvider) =>
                {
                    foreach (var net in btcPayNetworkProvider.GetAll().OfType<SPRYPayNetwork>())
                    {
                        options.ExternalServices.Load(net.CryptoCode, configuration);
                    }

                    options.ExternalServices.LoadNonCryptoServices(configuration);

                    var services = configuration.GetOrDefault<string>("externalservices", null);
                    if (services != null)
                    {
                        foreach (var service in services.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => (p, SeparatorIndex: p.IndexOf(':', StringComparison.OrdinalIgnoreCase)))
                            .Where(p => p.SeparatorIndex != -1)
                            .Select(p => (Name: p.p.Substring(0, p.SeparatorIndex),
                                Link: p.p.Substring(p.SeparatorIndex + 1))))
                        {
                            if (Uri.TryCreate(service.Link, UriKind.RelativeOrAbsolute, out var uri))
                                options.OtherExternalServices.AddOrReplace(service.Name, uri);
                        }
                    }
                });
            services.TryAddSingleton(o => configuration.ConfigureNetworkProvider(logs));

            services.TryAddSingleton<AppService>();
            services.AddTransient<PluginService>();
            services.AddSingleton<IPluginHookService, PluginHookService>();
            services.TryAddTransient<Safe>();
            services.TryAddSingleton<Ganss.XSS.HtmlSanitizer>(o =>
            {

                var htmlSanitizer = new Ganss.XSS.HtmlSanitizer();


                htmlSanitizer.RemovingAtRule += (sender, args) =>
                {
                };
                htmlSanitizer.RemovingTag += (sender, args) =>
                {
                    if (args.Tag.TagName.Equals("img", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!args.Tag.ClassList.Contains("img-fluid"))
                        {
                            args.Tag.ClassList.Add("img-fluid");
                        }

                        args.Cancel = true;
                    }
                };

                htmlSanitizer.RemovingAttribute += (sender, args) =>
                {
                    if (args.Tag.TagName.Equals("img", StringComparison.InvariantCultureIgnoreCase) &&
                        args.Attribute.Name.Equals("src", StringComparison.InvariantCultureIgnoreCase) &&
                        args.Reason == Ganss.XSS.RemoveReason.NotAllowedUrlValue)
                    {
                        args.Cancel = true;
                    }
                };
                htmlSanitizer.RemovingStyle += (sender, args) => { args.Cancel = true; };
                htmlSanitizer.AllowedAttributes.Add("class");
                htmlSanitizer.AllowedTags.Add("iframe");
                htmlSanitizer.AllowedTags.Add("style");
                htmlSanitizer.AllowedTags.Remove("img");
                htmlSanitizer.AllowedAttributes.Add("webkitallowfullscreen");
                htmlSanitizer.AllowedAttributes.Add("allowfullscreen");
                return htmlSanitizer;
            });

            services.TryAddSingleton<LightningConfigurationProvider>();
            services.TryAddSingleton<LanguageService>();
            services.TryAddSingleton<NBXplorerDashboard>();
            services.AddSingleton<ISyncSummaryProvider, NBXSyncSummaryProvider>();
            services.TryAddSingleton<StoreRepository>();
            services.TryAddSingleton<PaymentRequestRepository>();
            services.TryAddSingleton<SPRYPayWalletProvider>();
            services.TryAddSingleton<WalletReceiveService>();
            services.AddSingleton<IHostedService>(provider => provider.GetService<WalletReceiveService>());
            services.TryAddSingleton<CurrencyNameTable>(CurrencyNameTable.Instance);
            services.TryAddSingleton<IFeeProviderFactory>(o => new NBXplorerFeeProviderFactory(o.GetRequiredService<ExplorerClientProvider>())
            {
                Fallback = new FeeRate(100L, 1)
            });

            services.Configure<MvcOptions>((o) =>
            {
                o.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(WalletId)));
                o.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(DerivationStrategyBase)));
            });

            services.AddSingleton<Services.NBXplorerConnectionFactory>();
            services.AddSingleton<IHostedService, Services.NBXplorerConnectionFactory>(o => o.GetRequiredService<Services.NBXplorerConnectionFactory>());
            services.AddSingleton<HostedServices.CheckConfigurationHostedService>();
            services.AddSingleton<IHostedService, HostedServices.CheckConfigurationHostedService>(o => o.GetRequiredService<CheckConfigurationHostedService>());
            services.AddSingleton<HostedServices.WebhookSender>();
            services.AddSingleton<IHostedService, WebhookSender>(o => o.GetRequiredService<WebhookSender>());
            services.AddSingleton<IHostedService, StoreEmailRuleProcessorSender>();
            services.AddHttpClient(WebhookSender.OnionNamedClient)
                .ConfigurePrimaryHttpMessageHandler<Socks5HttpClientHandler>();
            services.AddHttpClient(WebhookSender.LoopbackNamedClient)
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });


            services.AddSingleton<BitcoinLikePayoutHandler>();
            services.AddSingleton<IPayoutHandler>(provider => provider.GetRequiredService<BitcoinLikePayoutHandler>());
            services.AddSingleton<IPayoutHandler>(provider => provider.GetRequiredService<LightningLikePayoutHandler>());
            services.AddSingleton<LightningLikePayoutHandler>();

            services.AddHttpClient(LightningLikePayoutHandler.LightningLikePayoutHandlerOnionNamedClient)
                .ConfigurePrimaryHttpMessageHandler<Socks5HttpClientHandler>();
            services.AddSingleton<HostedServices.PullPaymentHostedService>();
            services.AddSingleton<IHostedService, HostedServices.PullPaymentHostedService>(o => o.GetRequiredService<PullPaymentHostedService>());

            services.AddSingleton<BitcoinLikePaymentHandler>();
            services.AddSingleton<IPaymentMethodHandler>(provider => provider.GetService<BitcoinLikePaymentHandler>());
            services.AddSingleton<IHostedService, NBXplorerListener>();

            services.AddSingleton<LightningLikePaymentHandler>();
            services.AddSingleton<IPaymentMethodHandler>(provider => provider.GetService<LightningLikePaymentHandler>());
            services.AddSingleton<LNURLPayPaymentHandler>();
            services.AddSingleton<IPaymentMethodHandler>(provider => provider.GetService<LNURLPayPaymentHandler>());
            services.AddSingleton<IUIExtension>(new UIExtension("LNURL/LightningAddressNav",
                "store-integrations-nav"));
            services.AddSingleton<IUIExtension>(new UIExtension("LNURL/LightningAddressOption",
                "store-integrations-list"));
            services.AddSingleton<IHostedService, LightningListener>();
            services.AddSingleton<IHostedService, LightningPendingPayoutListener>();

            services.AddSingleton<PaymentMethodHandlerDictionary>();

            services.AddSingleton<NotificationManager>();
            services.AddScoped<NotificationSender>();

            services.AddSingleton<IHostedService, NBXplorerWaiters>();
            services.AddSingleton<IHostedService, InvoiceEventSaverService>();
            services.AddSingleton<IHostedService, BitpayIPNSender>();
            services.AddSingleton<IHostedService, InvoiceWatcher>();
            services.AddSingleton<IHostedService, RatesHostedService>();
            services.AddSingleton<IHostedService, BackgroundJobSchedulerHostedService>();
            services.AddSingleton<IHostedService, AppHubStreamer>();
            services.AddSingleton<IHostedService, AppInventoryUpdaterHostedService>();
            services.AddSingleton<IHostedService, TransactionLabelMarkerHostedService>();
            services.AddSingleton<IHostedService, UserEventHostedService>();
            services.AddSingleton<IHostedService, DynamicDnsHostedService>();
            services.AddSingleton<IHostedService, PaymentRequestStreamer>();
            services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
            services.AddScoped<IAuthorizationHandler, CookieAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, BitpayAuthorizationHandler>();

            services.AddSingleton<IVersionFetcher, GithubVersionFetcher>();
            services.AddSingleton<IHostedService, NewVersionCheckerHostedService>();
            services.AddSingleton<INotificationHandler, NewVersionNotification.Handler>();

            services.AddSingleton<INotificationHandler, InvoiceEventNotification.Handler>();
            services.AddSingleton<INotificationHandler, PayoutNotification.Handler>();
            services.AddSingleton<INotificationHandler, ExternalPayoutTransactionNotification.Handler>();
            services.AddSingleton<IHostedService, DbMigrationsHostedService>();
#if DEBUG
            services.AddSingleton<INotificationHandler, JunkNotification.Handler>();
#endif    
            services.TryAddSingleton<ExplorerClientProvider>();
            services.AddSingleton<IExplorerClientProvider, ExplorerClientProvider>(x =>
                x.GetRequiredService<ExplorerClientProvider>());
            services.TryAddSingleton<Bitpay>(o =>
            {
                if (o.GetRequiredService<SPRYPayServerOptions>().NetworkType == ChainName.Mainnet)
                    return new Bitpay(new Key(), new Uri("https://bitpay.com/"));
                else
                    return new Bitpay(new Key(), new Uri("https://test.bitpay.com/"));
            });
            RegisterRateSources(services);
            services.TryAddSingleton<RateProviderFactory>();
            services.TryAddSingleton<RateFetcher>();

            services.TryAddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<BitpayAccessTokenController>();
            services.AddTransient<UIInvoiceController>();
            services.AddTransient<UIPaymentRequestController>();
            // Add application services.
            services.AddSingleton<EmailSenderFactory>();
            services.AddSingleton<InvoiceActivator>();

            //create a simple client which hooks up to the http scope
            services.AddScoped<SPRYPayServerClient, LocalSPRYPayServerClient>();
            //also provide a factory that can impersonate user/store id
            services.AddSingleton<ISPRYPayServerClientFactory, SPRYPayServerClientFactory>();
            services.AddPayoutProcesors();
            services.AddForms();

            services.AddAPIKeyAuthentication();
            services.AddSpryPayServerAuthenticationSchemes();
            services.AddAuthorization(o => o.AddSPRYPayPolicies());

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicies.All, p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            });
            services.AddRateLimits();
            services.AddLogging(logBuilder =>
            {
                var debugLogFile = SPRYPayServerOptions.GetDebugLog(configuration);
                if (!string.IsNullOrEmpty(debugLogFile))
                {
                    Serilog.Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .MinimumLevel.Is(SPRYPayServerOptions.GetDebugLogLevel(configuration))
                        .WriteTo.File(debugLogFile, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: MAX_DEBUG_LOG_FILE_SIZE, rollOnFileSizeLimit: true, retainedFileCountLimit: 1)
                        .CreateLogger();
                    logBuilder.AddProvider(new Serilog.Extensions.Logging.SerilogLoggerProvider(Log.Logger));
                }
            });

            services.AddSingleton<IObjectModelValidator, SkippableObjectValidatorProvider>();
            services.SkipModelValidation<RootedKeyPath>();
            services.SkipModelValidation<NodeInfo>();

            if (configuration.GetOrDefault<bool>("cheatmode", false))
            {
                services.AddSingleton<Cheater>();
                services.AddSingleton<IHostedService, Cheater>(o => o.GetRequiredService<Cheater>());
            }
            return services;
        }

        internal static void RegisterRateSources(IServiceCollection services)
        {
            // We need to be careful to only add exchanges which OnGetTickers implementation make only 1 request
            services.AddRateProviderExchangeSharp<ExchangeBinanceAPI>(new("binance", "Binance", "https://api.binance.com/api/v1/ticker/24hr"));
            services.AddRateProviderExchangeSharp<ExchangeBittrexAPI>(new("bittrex", "Bittrex", "https://bittrex.com/api/v1.1/public/getmarketsummaries"));
            services.AddRateProviderExchangeSharp<ExchangePoloniexAPI>(new("poloniex", "Poloniex", "https://poloniex.com/public?command=returnTicker"));
            services.AddRateProviderExchangeSharp<ExchangeNDAXAPI>(new("ndax", "NDAX", "https://ndax.io/api/returnTicker"));

            services.AddRateProviderExchangeSharp<ExchangeBitfinexAPI>(new("bitfinex", "Bitfinex", "https://api.bitfinex.com/v2/tickers?symbols=tSPRYUSD,tLTCUSD,tLTCSPRY,tETHUSD,tETHSPRY,tETCSPRY,tETCUSD,tRRTUSD,tRRTSPRY,tZECUSD,tZECSPRY,tXMRUSD,tXMRSPRY,tDSHUSD,tDSHSPRY,tSPRYEUR,tSPRYJPY,tXRPUSD,tXRPSPRY,tIOTUSD,tIOTSPRY,tIOTETH,tEOSUSD,tEOSSPRY,tEOSETH,tSANUSD,tSANSPRY,tSANETH,tOMGUSD,tOMGSPRY,tOMGETH,tNEOUSD,tNEOSPRY,tNEOETH,tETPUSD,tETPSPRY,tETPETH,tQTMUSD,tQTMSPRY,tQTMETH,tAVTUSD,tAVTSPRY,tAVTETH,tEDOUSD,tEDOSPRY,tEDOETH,tBTGUSD,tBTGSPRY,tDATUSD,tDATSPRY,tDATETH,tQSHUSD,tQSHSPRY,tQSHETH,tYYWUSD,tYYWSPRY,tYYWETH,tGNTUSD,tGNTSPRY,tGNTETH,tSNTUSD,tSNTSPRY,tSNTETH,tIOTEUR,tBATUSD,tBATSPRY,tBATETH,tMNAUSD,tMNASPRY,tMNAETH,tFUNUSD,tFUNSPRY,tFUNETH,tZRXUSD,tZRXSPRY,tZRXETH,tTNBUSD,tTNBSPRY,tTNBETH,tSPKUSD,tSPKSPRY,tSPKETH,tTRXUSD,tTRXSPRY,tTRXETH,tRCNUSD,tRCNSPRY,tRCNETH,tRLCUSD,tRLCSPRY,tRLCETH,tAIDUSD,tAIDSPRY,tAIDETH,tSNGUSD,tSNGSPRY,tSNGETH,tREPUSD,tREPSPRY,tREPETH,tELFUSD,tELFSPRY,tELFETH,tNECUSD,tNECSPRY,tNECETH,tSPRYGBP,tETHEUR,tETHJPY,tETHGBP,tNEOEUR,tNEOJPY,tNEOGBP,tEOSEUR,tEOSJPY,tEOSGBP,tIOTJPY,tIOTGBP,tIOSUSD,tIOSSPRY,tIOSETH,tAIOUSD,tAIOSPRY,tAIOETH,tREQUSD,tREQSPRY,tREQETH,tRDNUSD,tRDNSPRY,tRDNETH,tLRCUSD,tLRCSPRY,tLRCETH,tWAXUSD,tWAXSPRY,tWAXETH,tDAIUSD,tDAISPRY,tDAIETH,tAGIUSD,tAGISPRY,tAGIETH,tBFTUSD,tBFTSPRY,tBFTETH,tMTNUSD,tMTNSPRY,tMTNETH,tODEUSD,tODESPRY,tODEETH,tANTUSD,tANTSPRY,tANTETH,tDTHUSD,tDTHSPRY,tDTHETH,tMITUSD,tMITSPRY,tMITETH,tSTJUSD,tSTJSPRY,tSTJETH,tXLMUSD,tXLMEUR,tXLMJPY,tXLMGBP,tXLMSPRY,tXLMETH,tXVGUSD,tXVGEUR,tXVGJPY,tXVGGBP,tXVGSPRY,tXVGETH,tBCIUSD,tBCISPRY,tMKRUSD,tMKRSPRY,tMKRETH,tKNCUSD,tKNCSPRY,tKNCETH,tPOAUSD,tPOASPRY,tPOAETH,tEVTUSD,tLYMUSD,tLYMSPRY,tLYMETH,tUTKUSD,tUTKSPRY,tUTKETH,tVEEUSD,tVEESPRY,tVEEETH,tDADUSD,tDADSPRY,tDADETH,tORSUSD,tORSSPRY,tORSETH,tAUCUSD,tAUCSPRY,tAUCETH,tPOYUSD,tPOYSPRY,tPOYETH,tFSNUSD,tFSNSPRY,tFSNETH,tCBTUSD,tCBTSPRY,tCBTETH,tZCNUSD,tZCNSPRY,tZCNETH,tSENUSD,tSENSPRY,tSENETH,tNCAUSD,tNCASPRY,tNCAETH,tCNDUSD,tCNDSPRY,tCNDETH,tCTXUSD,tCTXSPRY,tCTXETH,tPAIUSD,tPAISPRY,tSEEUSD,tSEESPRY,tSEEETH,tESSUSD,tESSSPRY,tESSETH,tATMUSD,tATMSPRY,tATMETH,tHOTUSD,tHOTSPRY,tHOTETH,tDTAUSD,tDTASPRY,tDTAETH,tIQXUSD,tIQXSPRY,tIQXEOS,tWPRUSD,tWPRSPRY,tWPRETH,tZILUSD,tZILSPRY,tZILETH,tBNTUSD,tBNTSPRY,tBNTETH,tABSUSD,tABSETH,tXRAUSD,tXRAETH,tMANUSD,tMANETH,tBBNUSD,tBBNETH,tNIOUSD,tNIOETH,tDGXUSD,tDGXETH,tVETUSD,tVETSPRY,tVETETH,tUTNUSD,tUTNETH,tTKNUSD,tTKNETH,tGOTUSD,tGOTEUR,tGOTETH,tXTZUSD,tXTZSPRY,tCNNUSD,tCNNETH,tBOXUSD,tBOXETH,tTRXEUR,tTRXGBP,tTRXJPY,tMGOUSD,tMGOETH,tRTEUSD,tRTEETH,tYGGUSD,tYGGETH,tMLNUSD,tMLNETH,tWTCUSD,tWTCETH,tCSXUSD,tCSXETH,tOMNUSD,tOMNSPRY,tINTUSD,tINTETH,tDRNUSD,tDRNETH,tPNKUSD,tPNKETH,tDGBUSD,tDGBSPRY,tBSVUSD,tBSVSPRY,tBABUSD,tBABSPRY,tWLOUSD,tWLOXLM,tVLDUSD,tVLDETH,tENJUSD,tENJETH,tONLUSD,tONLETH,tRBTUSD,tRBTSPRY,tUSTUSD,tEUTEUR,tEUTUSD,tGSDUSD,tUDCUSD,tTSDUSD,tPAXUSD,tRIFUSD,tRIFSPRY,tPASUSD,tPASETH,tVSYUSD,tVSYSPRY,tZRXDAI,tMKRDAI,tOMGDAI,tBTTUSD,tBTTSPRY,tSPRYUST,tETHUST,tCLOUSD,tCLOSPRY,tIMPUSD,tIMPETH,tLTCUST,tEOSUST,tBABUST,tSCRUSD,tSCRETH,tGNOUSD,tGNOETH,tGENUSD,tGENETH,tATOUSD,tATOSPRY,tATOETH,tWBTUSD,tXCHUSD,tEUSUSD,tWBTETH,tXCHETH,tEUSETH,tLEOUSD,tLEOSPRY,tLEOUST,tLEOEOS,tLEOETH,tASTUSD,tASTETH,tFOAUSD,tFOAETH,tUFRUSD,tUFRETH,tZBTUSD,tZBTUST,tOKBUSD,tUSKUSD,tGTXUSD,tKANUSD,tOKBUST,tOKBETH,tOKBSPRY,tUSKUST,tUSKETH,tUSKSPRY,tUSKEOS,tGTXUST,tKANUST,tAMPUSD,tALGUSD,tALGSPRY,tALGUST,tSPRYXCH,tSWMUSD,tSWMETH,tTRIUSD,tTRIETH,tLOOUSD,tLOOETH,tAMPUST,tDUSK:USD,tDUSK:SPRY,tUOSUSD,tUOSSPRY,tRRBUSD,tRRBUST,tDTXUSD,tDTXUST,tAMPSPRY,tFTTUSD,tFTTUST,tPAXUST,tUDCUST,tTSDUST,tSPRY:CNHT,tUST:CNHT,tCNH:CNHT,tCHZUSD,tCHZUST,tSPRYF0:USTF0,tETHF0:USTF0"));
            services.AddRateProviderExchangeSharp<ExchangeOKExAPI>(new("okex", "OKEx", "https://www.okex.com/api/futures/v3/instruments/ticker"));
            services.AddRateProviderExchangeSharp<ExchangeCoinbaseAPI>(new("coinbasepro", "Coinbase Pro", "https://api.pro.coinbase.com/products"));


            // Handmade providers
            services.AddRateProvider<HitSPRYRateProvider>();
            services.AddRateProvider<CoinGeckoRateProvider>();
            services.AddRateProvider<KrakenExchangeRateProvider>();
            services.AddRateProvider<ByllsRateProvider>();
            services.AddRateProvider<BudaRateProvider>();
            services.AddRateProvider<BitbankRateProvider>();
            services.AddRateProvider<BitpayRateProvider>();
            services.AddRateProvider<RipioExchangeProvider>();
            services.AddRateProvider<CryptoMarketExchangeRateProvider>();
            services.AddRateProvider<BitflyerRateProvider>();
            services.AddRateProvider<YadioRateProvider>();
            services.AddRateProvider<BtcTurkRateProvider>();

            // Broken
            // Providers.Add("argoneum", new ArgoneumRateProvider(_httpClientFactory?.CreateClient("EXCHANGE_ARGONEUM")));

            // Those exchanges make too many requests, exchange sharp do not parallelize so it is too slow...
            //AddExchangeSharpProviders<ExchangeGeminiAPI>("gemini");
            //AddExchangeSharpProviders<ExchangeBitstampAPI>("bitstamp");
            //AddExchangeSharpProviders<ExchangeBitMEXAPI>("bitmex");
        }

        public static void AddRateProvider<T>(this IServiceCollection services) where T : class, IRateProvider
        {
            services.AddSingleton<IRateProvider, T>();
        }
        public static void AddRateProviderExchangeSharp<T>(this IServiceCollection services, RateSourceInfo rateInfo) where T : ExchangeAPI
        {
            services.AddSingleton<IRateProvider, ExchangeSharpRateProvider<T>>(o =>
            {
                var instance = ActivatorUtilities.CreateInstance<ExchangeSharpRateProvider<T>>(o);
                instance.RateSourceInfo = rateInfo;
                return instance;
            });
        }

        private static void AddSettingsAccessor<T>(IServiceCollection services) where T : class, new()
        {
            services.TryAddSingleton<ISettingsAccessor<T>, SettingsAccessor<T>>();
            services.AddSingleton<IHostedService>(provider => (SettingsAccessor<T>)provider.GetRequiredService<ISettingsAccessor<T>>());
            services.AddSingleton<IStartupTask>(provider => (SettingsAccessor<T>)provider.GetRequiredService<ISettingsAccessor<T>>());
            // Singletons shouldn't reference the settings directly, but ISettingsAccessor<T>, since singletons won't have refreshed values of the setting
            services.AddTransient<T>(provider => provider.GetRequiredService<ISettingsAccessor<T>>().Settings);
        }

        public static void SkipModelValidation<T>(this IServiceCollection services)
        {
            services.AddSingleton<SkippableObjectValidatorProvider.ISkipValidation, SkippableObjectValidatorProvider.SkipValidationType<T>>();
        }
        private const long MAX_DEBUG_LOG_FILE_SIZE = 2000000; // If debug log is in use roll it every N MB.
        private static void AddSpryPayServerAuthenticationSchemes(this IServiceCollection services)
        {
            services.AddAuthentication()
                .AddBitpayAuthentication()
                .AddAPIKeyAuthentication();
        }

        public static IApplicationBuilder UsePayServer(this IApplicationBuilder app)
        {
            app.UseMiddleware<GreenfieldMiddleware>();
            app.UseMiddleware<SPRYPayMiddleware>();
            return app;
        }
        public static IApplicationBuilder UseHeadersOverride(this IApplicationBuilder app)
        {
            app.UseMiddleware<HeadersOverrideMiddleware>();
            return app;
        }
    }
}
