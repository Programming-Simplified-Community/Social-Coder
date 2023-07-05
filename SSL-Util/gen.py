"""
Certificate Authority Name:
    Name of the certificate authority.
    Example: "Social Coder" --> produces a file "SocialCoderCA.crt"

Certificate Authority Password:
    Passphrase to apply to certificate authority file/key

Community Name:
    The Organization portion of the subject

Days Valid:
    Number of days in which the certificate produced is valid


"""
import os

certificate_authority_name = os.environ.get("CERT_CA_AUTH_NAME", "SocialCoderCA")
certificate_authority_password = os.environ.get("CERT_CA_AUTH_PASS", "Testing1234")
certificate_community_name = os.environ.get("CERT_COMMUNITY_NAME", "ProgrammingSimplifiedCommunity")
certificate_days_valid = os.environ.get("CERT_DAYS_VALID", 365)
certificate_public_name = os.environ.get("CERT_PUBLIC_NAME", "SocialCoder")

output_cert_name = certificate_public_name.replace(" ", "")
output_ca_name = certificate_authority_name.replace(" ", "")

subject = os.environ.get("SUBJECT", f"/C=US/ST=DC/O={certificate_community_name}/CN={certificate_public_name}")

"""
    Generating CA chain
"""
os.system(f"openssl genrsa -aes256 -out {output_ca_name}.key -passout pass:{certificate_authority_password} 4096")
os.system(f"openssl req -new -x509 -sha256 -days {certificate_days_valid} "
          f"-subj \"{subject}\" -key {output_ca_name}.key -out {output_ca_name}.crt "
          f"-passin pass:{certificate_authority_password}")

"""
    Generating Social Coder specific certificate
"""
os.system(f"openssl genrsa -out {output_cert_name}.key 4096")
os.system(f"openssl req -new -sha256 -subj \"{subject}\" -key {output_ca_name}.key "
          f"-passin pass:{certificate_authority_password} -out {output_cert_name}.csr")

with open(f"{output_cert_name}-extfile.cnf", "w") as file:
    file.writelines([f"subjectAltName=DNS:{certificate_public_name}"])

os.system(
    f"openssl x509 -req -sha256 -days {certificate_days_valid} -in {output_cert_name}.csr -CA {output_ca_name}.crt -CAkey {output_ca_name}.key "
    f"-out {output_cert_name}.crt -CAcreateserial -passin pass:{certificate_authority_password} "
    f"-extfile {output_cert_name}-extfile.cnf"
)

items_to_remove = [
    f"{output_ca_name}.srl",
    f"{output_ca_name}.key",
    f"{output_cert_name}-extfile.cnf"
]

for item in items_to_remove:
    os.remove(item)