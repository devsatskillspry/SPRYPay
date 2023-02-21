#!/usr/bin/env bash

echo "$(/sbin/ip route|awk '/default/ { print $3 }')  host.docker.internal" >> /etc/hosts

if [ -f "$SPRYPAY_SSHAUTHORIZEDKEYS" ] && [[ "$SPRYPAY_SSHKEYFILE" ]]; then
    if ! [ -f "$SPRYPAY_SSHKEYFILE" ] || ! [ -f "$SPRYPAY_SSHKEYFILE.pub" ]; then
        rm -f "$SPRYPAY_SSHKEYFILE" "$SPRYPAY_SSHKEYFILE.pub"
        echo "Creating SPRYPay Server SSH key File..."
        ssh-keygen -t ed25519 -f "$SPRYPAY_SSHKEYFILE" -q -P "" -m PEM -C sprypayserver > /dev/null
        # Let's make sure the SSHAUTHORIZEDKEYS doesn't have our key yet
        # Because the file is mounted, set -i does not work
        sed '/sprypayserver$/d' "$SPRYPAY_SSHAUTHORIZEDKEYS" > "$SPRYPAY_SSHAUTHORIZEDKEYS.new"
        cat "$SPRYPAY_SSHAUTHORIZEDKEYS.new" > "$SPRYPAY_SSHAUTHORIZEDKEYS"
        rm -rf "$SPRYPAY_SSHAUTHORIZEDKEYS.new"
    fi

    if [ -f "$SPRYPAY_SSHKEYFILE.pub" ] && \
       ! grep -q "sprypayserver$" "$SPRYPAY_SSHAUTHORIZEDKEYS"; then
        echo "Adding SPRYPay Server SSH key to authorized keys"
        cat "$SPRYPAY_SSHKEYFILE.pub" >> "$SPRYPAY_SSHAUTHORIZEDKEYS"
    fi
fi

exec dotnet SPRYPayServer.dll
