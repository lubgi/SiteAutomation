using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SiteAutomation
{
    [Serializable]
    public abstract class AbstractModel //: IModel 
    {

        [XmlIgnore] public virtual int Id { get; set; }
        [XmlIgnore] public virtual string Model { get; set; }
        [XmlIgnore] public virtual string Name { get; set; }
        [XmlIgnore] public virtual int Price { get; set; }
        [XmlIgnore] public virtual string Description { get; set; }
        [XmlIgnore] public virtual string Image { get; set; }
        [XmlIgnore] public virtual string Vendor { get; set; }
        [XmlIgnore] public Category Category { get; set; }


        public AbstractModel()
        {
        }

        public AbstractModel(DataRow row)
        {
            Id          = row.Field<int>("Id");
            Model       = row.Field<string>("Model");
            Name        = row.Field<string>("Name");
            Price       = Convert.ToInt32(row.Field<decimal>("Price"));
            Description = row.Field<string>("Description");
            Image       = row.Field<string>("Image");
            Vendor      = row.Field<string>("Vendor");

            Category = new Category(row);
        }

        protected static DataTable GetModelsTable(int productId = 0)
        {
            string query =
                    @"SELECT oc_product.product_id AS Id, oc_product.model AS Model, oc_product.price AS Price, oc_product.quantity AS Quantity, oc_product.stock_status_id As StockStatusId,
                                    oc_product.image AS Image, oc_product.ean AS Sku, oc_product.manufacturer_id AS ManufacturerId, oc_product.suppler_code AS SupplerCode,
                                    oc_product.date_modified AS DateModified, oc_manufacturer.name AS Vendor, oc_product_to_category.category_id AS CategoryId,
                                    oc_category_description.name AS CategoryName, oc_product_description.description AS Description, oc_product_description.name AS Name,
                                    oc_seo_url.keyword AS Link                                     

                             FROM oc_product

                             LEFT JOIN oc_manufacturer ON oc_product.manufacturer_id = oc_manufacturer.manufacturer_id
                             LEFT JOIN (SELECT * FROM oc_product_to_category ORDER BY oc_product_to_category.category_id DESC) as oc_product_to_category ON oc_product.product_id = oc_product_to_category.product_id
                             LEFT JOIN oc_category_description ON oc_product_to_category.category_id = oc_category_description.category_id
                             LEFT JOIN oc_product_description ON oc_product.product_id = oc_product_description.product_id
                             LEFT JOIN oc_seo_url ON oc_seo_url.query = CONCAT('product_id'+oc_product.product_id)
                             
                             WHERE oc_product.product_id" + (productId == 0 ? " != " : " == ") + productId + "\n" +
                    "GROUP BY Id";
                return GetDataTable(query);
            
        }

        protected static DataTable GetAttributesTable(int productId)
        {
            string query =
                @$"SELECT oc_product_attribute.attribute_id AS Id, oc_product_attribute.text as Value, oc_attribute_description.name AS Name

                   FROM oc_product_attribute
                   LEFT JOIN oc_attribute_description on oc_attribute_description.attribute_id = oc_product_attribute.attribute_id

                   WHERE oc_product_attribute.product_id = {productId}";

            return GetDataTable(query);
        }

        protected static DataTable GetImagesTable(int productId)
        {
            string query = @$"SELECT image As Link, sort_order as SortOrder 
                              FROM oc_product_image 
                              WHERE product_id = {productId} 
                              ORDER BY sort_order DESC";

            return GetDataTable(query);
        }


        private static DataTable GetDataTable(string query)
        {
            DataTable dataTable = new DataTable();

            using MySqlConnection  connection  = DBConnection.GetMySQLConnection();
            using MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, connection);
            dataAdapter.Fill(dataTable);
            
            return dataTable;
        }
    }


    public class Image
    {
        public string Link { get; set; }
        public int SortOrder { get; set; }
        public bool IsMain => SortOrder == -1;
        

        public Image(DataRow row)
        {
            Link      = row.Field<string>("Link");
            SortOrder = row.Field<int>("SortOrder");
        }

        public Image(string link)
        {
            Link      = link;
            SortOrder = -1;
        }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Category(DataRow row)
        {
            Id   = row.Field<int>("CategoryId");
            Name = row.Field<string>("CategoryName");
        }
        public Category(){}
    }

    public class Attribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public Attribute(DataRow row)
        {
            Id    = row.Field<int>("Id");
            Name  = row.Field<string>("Name");
            Value = row.Field<string>("Value");
        }
    }
}