using LibraryOrganizer.Models;
using System.ComponentModel.DataAnnotations;

namespace LibraryOrganizer.Attributes
{
    public class RequiredIfPartOfSetAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var book = (Book)validationContext.ObjectInstance;
            if (book.IsPartOfSet && book.SetId == null)
            {
                return new ValidationResult("SetId is required when the book is part of a set.");
            }
            return ValidationResult.Success;
        }
    }
}