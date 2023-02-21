window.SPRYPayShopifyIntegrationModule = function () {
    const pageElements = document.querySelector.bind(document);
    const insertElement = (document.querySelectorAll.bind(document),
        (e,
            n) => {
            n.parentNode.insertBefore(e,
                n.nextSibling)
        });

    // execute SPRYPayShopifyIntegrationModule as soon as possible
    var paymentMethod = pageElements(".payment-method-list__item__info");
    if (null === paymentMethod) {
        return void setTimeout(() => {
            window.SPRYPayShopifyIntegrationModule();
        }, 10);
    }

    if (!window.sprypay) {
        throw new Error("The SPRYPay modal js was not loaded on this page.");
    }
    if (!window.Shopify) {
        throw new Error("The Shopify global object was not loaded on this page.");
    }
    if (!window.SPRYPAYSERVER_URL || !window.STORE_ID) {
        throw new Error("The SPRYPAYSERVER_URL STORE_ID global vars were not set on this page.");
    }
    const shopify_order_id = Shopify.checkout.order_id;
    const btcPayServerUrl = window.SPRYPAYSERVER_URL;
    const storeId = window.STORE_ID;
    var currentInvoiceData;
    var modalShown = false;

    let buttonElement = null;

    var pageItems = {
        mainHeader: pageElements("#main-header"),
        orderConfirmed: pageElements(".os-step__title"),
        orderConfirmedDescription: pageElements(".os-step__description"),
        continueButton: pageElements(".step__footer__continue-btn"),
        checkMarkIcon: pageElements(".os-header__hanging-icon"),
        orderStatus: pageElements(".os-header__title"),
        paymentMethod: pageElements(".payment-method-list__item__info"),
        price: pageElements(".payment-due__price"),
        finalPrice: pageElements(".total-recap__final-price"),
        orderNumber: pageElements(".os-order-number"),
    };

    function setOrderAsPaid() {
        document.title = document.title.replace("Review and pay!", "Thank you!");
        pageItems.mainHeader.innerText = "Thank you!",
        pageItems.orderConfirmed && (pageItems.orderConfirmed.style.display = "block"),
        pageItems.orderConfirmedDescription && (pageItems.orderConfirmedDescription.style.display = "block"),
        pageItems.continueButton && (pageItems.continueButton.style.visibility = "visible"),
        pageItems.checkMarkIcon && (pageItems.checkMarkIcon.style.visibility = "visible"),
        buttonElement && (buttonElement.style.display = "none");
    }

    function showPaymentInstructions() {
        document.title = document.title.replace("Thank you!", "Review and pay!");
        pageItems.mainHeader && (pageItems.mainHeader.innerText = "Review and pay!"),
        pageItems.continueButton && (pageItems.continueButton.style.visibility = "hidden"),
        pageItems.checkMarkIcon && (pageItems.checkMarkIcon.style.visibility = "hidden"),
        pageItems.orderConfirmed && (pageItems.orderConfirmed.style.display = "none"),
        pageItems.orderConfirmedDescription && (pageItems.orderConfirmedDescription.style.display = "none");
    }

    function getOrCheckInvoice(backgroundCheck) {
        const url = btcPayServerUrl + "/stores/" + storeId + "/plugins/shopify/" + shopify_order_id+"?amount="+Shopify.checkout.payment_due+ (backgroundCheck ? "&checkonly=true" : "");
        return fetch(url, {
            method: "GET",
            mode: "cors", // no-cors, cors, *same-origin,
            headers: {
                "Content-Type": "application/json",
                "accept": "application/json",
            }
        })
            .then(function (response) {
                return response.json();
            }).catch(function () {
                if (!backgroundCheck)
                    alert("Could not initiate SPRYPay Server payment method, try again later.");
            })
    }

    function onPayButtonClicked() {
        buttonElement.innerHTML = "<span>Displaying Invoice...</span>";

        getOrCheckInvoice().then(handleInvoiceData).catch(fail.bind(this));
    }

    function handleInvoiceData(data, opts) {
        currentInvoiceData = data;
        if (!currentInvoiceData) {
            if (modalShown) {
                window.sprypay.hideFrame();
                fail();
            }else if(opts && opts.backgroundCheck){
                injectPaymentButtonHtml();
            }else{
                fail();
            }
            return;
        }
        if (["complete", "confirmed", "paid"].indexOf(currentInvoiceData.status.toLowerCase()) >= 0) {
            setOrderAsPaid();
        } else if (["invalid", "expired"].indexOf(currentInvoiceData.status.toLowerCase()) >= 0) {
            fail();
        } else if (!opts || !opts.backgroundCheck) {
            showModal();
        } 
    }

    function showModal() {
        if (currentInvoiceData && !modalShown) {
            modalShown = true;
            window.sprypay.setApiUrlPrefix(btcPayServerUrl);

            window.sprypay.onModalReceiveMessage(function (evt) {
                if (evt && evt.invoiceId && evt.status) {
                    currentInvoiceData = evt;
                }
            });

            window.sprypay.onModalWillEnter(function () {
                modalShown = true;
            });

            window.sprypay.onModalWillLeave(function () {
                modalShown = false;
                getOrCheckInvoice(true).then(function (d) {
                    buttonElement.innerHTML = payButtonHtml;
                    handleInvoiceData(d, {backgroundCheck: true})
                });
            });
            sprypay.appendAndShowInvoiceFrame(currentInvoiceData.invoiceId);
        }
    }

    function fail() {
        currentInvoiceData = null;
        buttonElement.innerHTML = payButtonHtml;
    }

    const payButtonHtml = '<button class="" onclick="onPayButtonClicked()" style="width:210px; border: none; outline: none;"><img src="' + btcPayServerUrl + '/img/paybutton/pay.svg"></button>';

    function injectPaymentButtonHtml() {
        // Payment button that opens modal
        buttonElement = document.getElementById("sprypayserver-pay");
        if (buttonElement) {
            return;
        }
        buttonElement = document.createElement("div");
        buttonElement.id = "sprypayserver-pay";
        buttonElement.innerHTML = payButtonHtml;
        insertElement(buttonElement, pageItems.orderConfirmed);
    }

    if (["bitcoin", "btc", "sprypayserver", "sprypay server"].filter(value => pageItems.paymentMethod.innerText.toLowerCase().indexOf(value) !== -1).length === 0) {
        return;
    }
    showPaymentInstructions();
    window.onPayButtonClicked = onPayButtonClicked.bind(this);
    getOrCheckInvoice(true).then(function (d) {
        injectPaymentButtonHtml();
        handleInvoiceData(d, {backgroundCheck: true})
    });

};

window.SPRYPayShopifyIntegrationModule();
