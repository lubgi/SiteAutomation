using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SiteAutomation
{
    public class OfferModel : AbstractModel
    {
        public int Quantity { get; set; } = 0;
        public bool Available => Quantity > 0;
        public List<Attribute> Attributes { get; set; } = new List<Attribute>();
        public List<Image> Images { get; set; } = new List<Image>();

        public static Dictionary<int, string> Categories = new Dictionary<int, string>();

        public OfferModel(DataRow row) : base(row)
        {
            Quantity = row.Field<int>("Quantity");
            GetAttributes();
            GetImages();
            Categories.TryAdd(Category.Id, Category.Name);
        }

        public static List<OfferModel> GetAllOfferModels()
        {
            List<OfferModel> offerModels = new List<OfferModel>();
            DataTable        modelsTable = GetModelsTable();

            foreach (DataRow row in modelsTable.Rows) offerModels.Add(new OfferModel(row));

            return offerModels;
        }

        private void GetAttributes()
        {
            DataTable attributesTable = GetAttributesTable(Id);
            foreach (DataRow row in attributesTable.Rows) Attributes.Add(new Attribute(row));
        }

        private void GetImages()
        {
            DataTable imagesTable = GetImagesTable(Id);
            foreach (DataRow row in imagesTable.Rows) Images.Add(new Image(row));
        }

        //public static OfferModel Select(int productId)
        //{
        //    return new OfferModel();
        //}

        //public static OfferModel RowToModel(DataRow dataRow)
        //{
        //    OfferModel offerModel = new OfferModel()
        //    {

        //    }

        //}
    }
}