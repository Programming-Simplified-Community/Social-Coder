using System.ComponentModel;

namespace SocialCoder.CLI;

[DisplayName("SSL Cert Settings")]
public class SocialCoderCertSettings
{
    /// <summary>
    /// Name of certificate authority file
    /// </summary>
    [DisplayName("CERT_CA_AUTH_NAME")] 
    [Description("Name of certificate authority (for testing)")]
    public string CertificateAuthorityName { get; set; } = "SocialCoderCA";

    /// <summary>
    /// Password to utilize for certificate authority
    /// </summary>
    [Description("The passphrase to associate with the certificate authority")]
    [DisplayName("CERT_CA_AUTH_PASS")] 
    public string CertificateAuthorityPassword { get; set; } = "Testing1234";

    /// <summary>
    /// Attribute to add within the Subject (Considered the "Org")
    /// </summary>
    [DisplayName("CERT_COMMUNITY_NAME")]
    [Description("Name of the community to associate certificate to. This will appear as the \"Organization\" within the subject.")]
    public string CommunityName { get; set; } = "ProgrammingSimplifiedCommunity";

    /// <summary>
    /// Number of days the certificate is valid for
    /// </summary>
    [Description("Number of days the generated certificates will be valid for")]
    [DisplayName("CERT_DAYS_VALID")] 
    public int DaysValid { get; set; } = 180;

    /// <summary>
    /// Common Name for DNS (SubjectAltName
    /// </summary>
    [DisplayName("CERT_PUBLIC_NAME")] 
    [Description("The public facing name for the certificate")]
    public string PublicName { get; set; } = "SocialCoder";
    
    public override string ToString()
        => $"""

Current settings
[cyan]CERT_CA_AUTH_NAME[/]: {CertificateAuthorityName}
[cyan]CERT_CA_AUTH_PASS[/]: {CertificateAuthorityPassword}
[cyan]CERT_COMMUNITY_NAME[/]: {CommunityName}
[cyan]CERT_DAYS_VALID[/]: {DaysValid}
[cyan]CERT_PUBLIC_NAME[/]: {PublicName}

""";
}

[DisplayName("Database Settings")]
public class SocialCoderDatabaseSettings
{
    /// <summary>
    /// Location on disk to mount database to (for persistent storage)
    /// </summary>
    [DisplayName("DB_PATH")]
    [Description("The path on host filesystem to mount the database to.")]
    public string? PersistentPath { get; set; }
    
    /// <summary>
    /// Password to use for dev database
    /// </summary>
    [DisplayName("DB_PASSWORD")]
    [Description("The password to use for the database.")]
    public string Password { get; set; } = "my-awesome-password";
    
    /// <summary>
    /// Database name within the container
    /// </summary>
    [DisplayName("DB_NAME")]
    [Description("The database name to use within the DB server")]
    public string Name { get; set; } = "social-coder";
    
    /// <summary>
    /// Host/IP of database
    /// </summary>
    [DisplayName("DB_HOST")]
    [Description("Address to the DB. Localhost by default.")]
    public string Host { get; set; } = "localhost";
    
    /// <summary>
    /// User for database
    /// </summary>
    [DisplayName("DB_USER")]
    [Description("User to login as")]
    public string User { get; set; } = "social-coder";
    
    /// <summary>
    /// Docker image + version to utilize for database
    /// </summary>
    [DisplayName("DB_DOCKER_IMAGE")]
    [Description("Docker image to utilize. Can specify version. Default is \"postgres:latest\"")]
    public string DockerImage { get; set; } = "postgres:latest";

    public override string ToString()
        => $"""

Current settings
[cyan]Persistent Path[/]: {PersistentPath}
[cyan]Password[/]: {Password}
[cyan]Name[/]: {Name}
[cyan]Host[/]: {Host}
[cyan]User[/]: {User}
[cyan]DockerImage[/]: {DockerImage}

""";
}