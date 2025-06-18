using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace eUseControl.BeekeepingStore.Helpers
{
    public static class HtmlSanitizer
    {
        // Lista de tag-uri HTML permise pentru conținutul blog-ului
        private static readonly HashSet<string> AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "strong", "b", "em", "i", "u", "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li", "blockquote", "a", "img", "table", "thead", "tbody", "tr", "td", "th",
            "div", "span", "pre", "code", "hr", "sub", "sup"
        };

        // Lista de atribute permise pentru tag-uri
        private static readonly Dictionary<string, HashSet<string>> AllowedAttributes = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href", "title", "target" },
            ["img"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src", "alt", "title", "width", "height" },
            ["table"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "border", "cellpadding", "cellspacing" },
            ["td"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan" },
            ["th"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan" },
            ["div"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" },
            ["span"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" }
        };

        // Protocoale permise pentru link-uri
        private static readonly HashSet<string> AllowedProtocols = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "http", "https", "mailto"
        };

        /// <summary>
        /// Sanitizează conținutul HTML pentru a preveni atacurile XSS
        /// </summary>
        /// <param name="html">Conținutul HTML de sanitizat</param>
        /// <returns>Conținutul HTML sanitizat</returns>
        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Elimină script-urile și alte tag-uri periculoase
            html = RemoveDangerousTags(html);

            // Elimină atributele periculoase (onclick, onload, etc.)
            html = RemoveDangerousAttributes(html);

            // Validează și curăță link-urile
            html = SanitizeLinks(html);

            // Elimină tag-urile nepermise
            html = RemoveUnallowedTags(html);

            return html;
        }

        /// <summary>
        /// Elimină tag-urile periculoase complet
        /// </summary>
        private static string RemoveDangerousTags(string html)
        {
            var dangerousTags = new[]
            {
                "script", "object", "embed", "form", "input", "button", "textarea", "select",
                "iframe", "frame", "frameset", "applet", "base", "link", "meta", "style"
            };

            foreach (var tag in dangerousTags)
            {
                // Elimină tag-urile cu conținutul lor
                html = Regex.Replace(html, $@"<{tag}[^>]*>.*?</{tag}>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                // Elimină tag-urile self-closing
                html = Regex.Replace(html, $@"<{tag}[^>]*/>", "", RegexOptions.IgnoreCase);
                // Elimină tag-urile fără închidere
                html = Regex.Replace(html, $@"<{tag}[^>]*>", "", RegexOptions.IgnoreCase);
            }

            return html;
        }

        /// <summary>
        /// Elimină atributele periculoase din tag-uri
        /// </summary>
        private static string RemoveDangerousAttributes(string html)
        {
            // Elimină toate atributele care încep cu "on" (onclick, onload, etc.)
            html = Regex.Replace(html, @"\s+on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"\s+on\w+\s*=\s*[^>\s]+", "", RegexOptions.IgnoreCase);

            // Elimină atributele javascript:
            html = Regex.Replace(html, @"\s+\w+\s*=\s*[""']javascript:[^""']*[""']", "", RegexOptions.IgnoreCase);

            // Elimină atributele style cu expresii periculoase
            html = Regex.Replace(html, @"\s+style\s*=\s*[""'][^""']*expression[^""']*[""']", "", RegexOptions.IgnoreCase);

            return html;
        }

        /// <summary>
        /// Sanitizează link-urile pentru a permite doar protocoale sigure
        /// </summary>
        private static string SanitizeLinks(string html)
        {
            return Regex.Replace(html, @"<a\s+[^>]*href\s*=\s*[""']([^""']+)[""'][^>]*>", match =>
            {
                var href = match.Groups[1].Value;

                // Verifică dacă link-ul are un protocol valid
                if (Uri.TryCreate(href, UriKind.Absolute, out Uri uri))
                {
                    if (!AllowedProtocols.Contains(uri.Scheme.ToLower()))
                    {
                        // Înlocuiește cu un link sigur
                        return match.Value.Replace(href, "#");
                    }
                }
                else if (href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                {
                    // Elimină link-urile javascript
                    return match.Value.Replace(href, "#");
                }

                return match.Value;
            }, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Elimină tag-urile care nu sunt în lista celor permise
        /// </summary>
        private static string RemoveUnallowedTags(string html)
        {
            return Regex.Replace(html, @"<(/?)(\w+)[^>]*>", match =>
            {
                var isClosing = !string.IsNullOrEmpty(match.Groups[1].Value);
                var tagName = match.Groups[2].Value.ToLower();

                if (!AllowedTags.Contains(tagName))
                {
                    return ""; // Elimină tag-ul complet
                }

                if (isClosing)
                {
                    return $"</{tagName}>"; // Tag de închidere simplu
                }

                // Pentru tag-urile de deschidere, păstrează doar atributele permise
                return SanitizeTagAttributes(match.Value, tagName);
            }, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Sanitizează atributele unui tag specific
        /// </summary>
        private static string SanitizeTagAttributes(string tagHtml, string tagName)
        {
            if (!AllowedAttributes.ContainsKey(tagName))
            {
                // Dacă tag-ul nu are atribute permise, returnează tag-ul simplu
                return $"<{tagName}>";
            }

            var allowedAttrs = AllowedAttributes[tagName];
            var result = $"<{tagName}";

            // Extrage și validează atributele
            var attributeMatches = Regex.Matches(tagHtml, @"(\w+)\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);

            foreach (Match attrMatch in attributeMatches)
            {
                var attrName = attrMatch.Groups[1].Value.ToLower();
                var attrValue = attrMatch.Groups[2].Value;

                if (allowedAttrs.Contains(attrName))
                {
                    // Sanitizează valoarea atributului
                    attrValue = SanitizeAttributeValue(attrName, attrValue);
                    result += $" {attrName}=\"{attrValue}\"";
                }
            }

            result += ">";
            return result;
        }

        /// <summary>
        /// Sanitizează valoarea unui atribut
        /// </summary>
        private static string SanitizeAttributeValue(string attributeName, string value)
        {
            // Elimină caracterele periculoase
            value = value.Replace("\"", "&quot;")
                        .Replace("'", "&#39;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;");

            // Validări specifice pentru anumite atribute
            if (attributeName == "href" && value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                return "#";
            }

            if (attributeName == "src" && value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            return value;
        }

        /// <summary>
        /// Sanitizează conținutul pentru comentarii (mai restrictiv)
        /// </summary>
        /// <param name="content">Conținutul comentariului</param>
        /// <returns>Conținutul sanitizat</returns>
        public static string SanitizeComment(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Pentru comentarii, permitem doar formatare de bază
            var allowedCommentTags = new[] { "p", "br", "strong", "b", "em", "i", "u" };

            // Elimină toate tag-urile HTML exceptând cele permise
            content = Regex.Replace(content, @"<(?!/?(?:" + string.Join("|", allowedCommentTags) + @")\b)[^>]*>", "", RegexOptions.IgnoreCase);

            // Elimină atributele din tag-urile rămase
            content = Regex.Replace(content, @"<(\w+)[^>]*>", "<$1>", RegexOptions.IgnoreCase);

            return content;
        }
    }
}