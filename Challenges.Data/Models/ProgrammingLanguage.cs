using System.ComponentModel.DataAnnotations;

namespace Challenges.Data.Models;

public enum ProgrammingLanguage
{
    Python,
    GO,
    NodeJS,
    
    [Display(Name="C#")]
    CSharp
}