using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace SiteAutomation
{
    [Serializable]
    public class ItemModel : AbstractModel
    {
        [XmlElement("id", Namespace = "http://base.google.com/ns/1.0")]
        public override string Model
        {
            get => base.Model;
            set => base.Model = value;
        }

        [XmlElement("title", Namespace = "http://base.google.com/ns/1.0")]
        public override string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        [XmlElement("description", Namespace = "http://base.google.com/ns/1.0")]
        public override string Description
        {
            get => base.Description;
            set => base.Description = value;
        }

        [XmlElement("link", Namespace = "http://base.google.com/ns/1.0")]
        public string Link { get; set; }

        [XmlElement("image_link", Namespace = "http://base.google.com/ns/1.0")]
        public override string Image
        {
            get => base.Image;
            set => base.Image = value;
        }

        [XmlElement("product_type", Namespace = "http://base.google.com/ns/1.0")]
        public string ProductType
        {
            get => Category.Name;
            set => Category.Name = value;
        }

        [XmlElement("condition", Namespace = "http://base.google.com/ns/1.0")]
        public readonly string Condition = "new";

        [XmlElement("availability", Namespace = "http://base.google.com/ns/1.0")]
        public string Availability { get; set; }

        [XmlElement("brand", Namespace = "http://base.google.com/ns/1.0")]
        public override string Vendor
        {
            get => base.Vendor;
            set => base.Vendor = value;
        }

        [XmlElement("gtin", Namespace = "http://base.google.com/ns/1.0")]
        public string Sku { get; set; }

        [XmlElement("identifier_exists")] public string IdentifierExists = "no";

        [XmlElement("price", Namespace = "http://base.google.com/ns/1.0")]
        public new string Price => base.Price + " UAH";


        public bool ShouldSerializeSku()
        {
            return !string.IsNullOrEmpty(Sku);
        }


        public bool ShouldSerializeIdentifierExists()
        {
            return !ShouldSerializeSku();
        }


        public ItemModel(DataRow row) : base(row)
        {
            Link = row.Field<string>("Link");
            Sku = ValidateBarcode(row.Field<string>("Sku"));
            Availability = StatusToAvailability[row.Field<Int32>("StockStatusId")];
        }
        public ItemModel(){}

        private static readonly Dictionary<int, string> StatusToAvailability = new Dictionary<int, string>
            {
                { 7, "in_stock" },
                { 8, "preorder" },
                { 5, "out_of_stock" },
                { 6, "backorder" },
                { 9, "backorder" }
            };


        public static List<ItemModel> GetAllItemModels()
        {
            DataTable modelsTable = GetModelsTable();

            return (from DataRow row in modelsTable.Rows select new ItemModel(row)).ToList();
        }


        //проверка штрихкода (просто меняем последнюю циферку в зависимости от суммы)
        private static string ValidateBarcode(string barcode)
        {
            if (!Regex.IsMatch(barcode, "[0-9]+"))
                return "";

            int sum = 0;

            for (int i = 0; i < barcode.Length - 1; i++)
                if (barcode.Length % 2 != 0)
                {
                    if (i % 2 != 0)
                        sum += (int)char.GetNumericValue(barcode[i]) * 3;
                    else
                        sum += (int)char.GetNumericValue(barcode[i]);
                }
                else
                {
                    if (i % 2 == 0)
                        sum += (int)char.GetNumericValue(barcode[i]) * 3;
                    else
                        sum += (int)char.GetNumericValue(barcode[i]);
                }

            sum = (10 - sum % 10) % 10;

            return barcode.Substring(0, barcode.Length - 1) + sum;
        }
    }
}