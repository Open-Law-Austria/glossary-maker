using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace glossary_maker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Usage: glossarymaker.exe <input-xml> <output-xml> <category> <category-slug> <uniquetag>");
                return;
            }

            var inputFile = args[0];
            var outputFile = args[1];
            var category = args[2];
            var categoryPretty = args[3];
            var uniqueTag = args[4];

            var terms = ReadGlossary(inputFile);
            var glossary = CreateGlossary(category, categoryPretty, uniqueTag, terms);
            glossary.Save(outputFile);
        }

        static Dictionary<string, string> ReadGlossary(string file)
        {
            var result = new Dictionary<string, string>();
            XDocument doc = XDocument.Load(file);
            
            var items = doc.Element("glossary").Elements("item");
            foreach(var item in items){
                result.Add(item.Element("term").Value, item.Element("description").Value);
            }

            return result;
        }

        static XDocument CreateGlossary(string category, string categoryPretty, string uniqueTag, Dictionary<string, string> terms)
        {

            XNamespace excerpt = "http://wordpress.org/export/1.2/excerpt";
            XNamespace content = "http://purl.org/rss/1.0/modules/content/";
            XNamespace wfw = "http://wellformedweb.org/CommentAPI/";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            XNamespace wp = "http://wordpress.org/export/1.2/";

            XElement channel = new XElement("channel",
                        new XElement("title", "Legal Glossary Austria"),
                        new XElement("link", "https://legal-glossary.com"),
                        new XElement("description", ""),
                        new XElement("pubDate", "Fri, 02 Oct 2020 08:51:38 +0000"),
                        new XElement("language", "en-US"),
                        new XElement(wp+"wxr_version", "1.2"),
                        new XElement(wp+"base_site_url", "https://legal-glossary.com"),
                        new XElement(wp+"base_blog_url", "https://legal-glossary.com"),
                        new XElement("generator", "https://wordpress.org/?v=5.5.1")
                    );

            XDocument doc = new XDocument(
                new XElement("rss",
                    new XAttribute("version", "2.0"),
                    new XAttribute(XNamespace.Xmlns+"excerpt", excerpt),
                    new XAttribute(XNamespace.Xmlns+"content", content),
                    new XAttribute(XNamespace.Xmlns+"wfw", wfw),
                    new XAttribute(XNamespace.Xmlns+"dc", dc),
                    new XAttribute(XNamespace.Xmlns+"wp", wp),
                    channel
                )
            );
            doc.Declaration = new XDeclaration("1.0", "utf-8", "true");

            foreach (var term in terms)
            {
                var termPretty = Uri.EscapeUriString(term.Key.Replace(" ", "-"));

                XElement element = new XElement("item",
                    new XElement("title", $"{term.Key} ({uniqueTag})"),
                    //new XElement("link", "https://legal-glossary.com/?encyclopedia=" + termPretty),
                    new XElement("pubDate", "Thu, 01 Oct 2020 15:40:18 +0000"),
                    new XElement(dc+"creator", new XCData("admin")),
                    // <guid isPermaLink="false">https://legal-glossary.com/?post_type=encyclopedia&#038;p=5</guid>
                    new XElement(content+"encoded", new XCData(term.Value)),
                    //new XElement("post_id", 5),
                    new XElement(wp+"post_date", new XCData("2020-10-01 15:40:18")),
                    new XElement(wp+"post_date_gmt", new XCData("2020-10-01 15:40:18")),
                    new XElement(wp+"comment_status", new XCData("closed")),
                    new XElement(wp+"ping_status", new XCData("closed")),
                    new XElement(wp+"post_name", new XCData($"{term.Key} ({uniqueTag})")),
                    new XElement(wp+"status", new XCData("publish")),
                    new XElement(wp+"post_type", new XCData("encyclopedia")),
                    new XElement(wp+"is_sticky", "0"),
                    new XElement("category",
                        new XAttribute("domain", "encyclopedia-tag"),
                        new XAttribute("nicename", categoryPretty),
                        new XCData(category)
                    )
                );

                channel.Add(element);
            }

            return doc;
        }
    }
}
