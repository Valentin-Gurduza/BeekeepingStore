using System;
using System.ComponentModel.DataAnnotations;

namespace eUseControl.BeekeepingStore.Helpers
{
    /// <summary>
    /// Atribut de validare pentru conținut HTML securizat
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SecureHtmlAttribute : ValidationAttribute
    {
        private readonly bool _allowRichContent;

        /// <summary>
        /// Constructor pentru atributul SecureHtml
        /// </summary>
        /// <param name="allowRichContent">Dacă să permită conținut HTML bogat (pentru blog posts) sau doar formatare de bază (pentru comentarii)</param>
        public SecureHtmlAttribute(bool allowRichContent = true)
        {
            _allowRichContent = allowRichContent;
            ErrorMessage = "Conținutul HTML conține elemente nepermise sau potențial periculoase.";
        }

        /// <summary>
        /// Validează conținutul HTML
        /// </summary>
        /// <param name="value">Valoarea de validat</param>
        /// <returns>True dacă conținutul este valid, false altfel</returns>
        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true; // Null/empty values are handled by Required attribute

            string htmlContent = value.ToString();

            try
            {
                // Sanitizează conținutul
                string sanitizedContent = _allowRichContent
                    ? HtmlSanitizer.SanitizeHtml(htmlContent)
                    : HtmlSanitizer.SanitizeComment(htmlContent);

                // Verifică dacă conținutul sanitizat este diferit de originalul
                // Dacă da, înseamnă că conținutul original avea elemente nepermise
                return sanitizedContent.Length >= (htmlContent.Length * 0.8); // Permite o diferență de până la 20%
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formatează mesajul de eroare
        /// </summary>
        /// <param name="name">Numele câmpului</param>
        /// <returns>Mesajul de eroare formatat</returns>
        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name);
        }
    }
}