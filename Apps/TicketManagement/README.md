# Local Configuration

Generate the certificate using the following steps:

## Create a private key

```console
openssl genpkey -algorithm RSA -out private_key.pem -pkeyopt rsa_keygen_bits:2048
```

## Create a self-signed certificate using the private key

```console
openssl req -new -x509 -days 365 -key private_key.pem -out certificate.crt -subj "/O=Government of the Province of British Columbia/CN=tickets.healthgateway.gov.bc.ca"
```

## Convert the private key and certificate to a PFX file

```console
openssl pkcs12 -export -inkey private_key.pem -in certificate.crt -out certificate.pfx -name "WaitingQueue Local Development"
```
