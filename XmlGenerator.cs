using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SiteAutomation
{
    public static class XmlGenerator
    {
        public static void GenerateRozetkaXml()
        {
            List<OfferModel> offerModels = OfferModel.GetAllOfferModels();
            string           siteLink    = ConfigurationManager.AppSettings["SiteLink"];

            XDocument xml = new XDocument(
                new XElement("yml_catalog",
                    new XAttribute("date", DateTime.Now.ToString("yyyy-MM-dd hh:mm")),
                    new XElement("shop",
                        new XElement("name", "Deposit"),
                        new XElement("company", "Deposit"),
                        new XElement("currencies",
                            new XElement("currency",
                                new XAttribute("id", "UAH"),
                                new XAttribute("rate", "1"))),
                        new XElement("categories",
                            OfferModel.Categories.Select(category => 
                            new XElement("category", category.Value,
                                new XAttribute("id", category.Key)))),
                        new XElement("offers", offerModels.Select(offer =>
                            new XElement("offer",
                                new XAttribute("id", offer.Model),
                                new XAttribute("available", offer.Available),
                                new XElement("price", offer.Price),
                                new XElement("currencyID", "UAH"),
                                new XElement("categoryID", offer.Category.Id),
                                new List<Image> { new Image(offer.Image)}.Concat(offer.Images).Select(image =>
                                new XElement("picture", siteLink + "image/" + image.Link)),
                                new XElement("vendor", offer.Vendor),
                                new XElement("stock_quantity", offer.Quantity),
                                new XElement("name", offer.Name),
                                new XElement("description", new XCData(offer.Description)),
                                    offer.Attributes.Select(attribute =>
                                new XElement("param", attribute.Value,
                                    new XAttribute("name", attribute.Name)))))))));
            
            xml.AddFirst(new XDocumentType("yml_catalog", "", "SYSTEM", "shops.dtd"));
            xml.Save("RozetkaPrice.xml");
        }

        public static void GenerateGoogleFeedXml()
        {
            Feed feed = new Feed
                        {
                            Link    = ConfigurationManager.AppSettings["SiteLink"],
                            Title   = ConfigurationManager.AppSettings["SiteName"],
                            Updated = DateTime.Now,
                            Items   = new Feed.Channel {ItemModels = ItemModel.GetAllItemModels()}
                        };
            feed.Serialize();

        }

    }
}
