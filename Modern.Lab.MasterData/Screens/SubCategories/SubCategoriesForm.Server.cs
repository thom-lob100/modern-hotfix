using System.Data;

using Modern.Lab.Hosting.Schema;
using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.MasterData
{
    public partial class SubCategoriesForm
    {
        // SUB_CATG_CD
        protected override DataTable RequestItems()
        {
            return this.RequestFields("SubCategoryAction", "MethodCommand", "SelectSubCategories").Table;
        }

        protected override DataActionResult InsertItem(object[] requestFields)
        {
            return this.RequestFields("SubCategoryAction", WithMethodCommand("InsertSubCategory", requestFields));
        }

        protected override DataActionResult UpdateItem(object[] requestFields)
        {
            return this.RequestFields("SubCategoryAction", WithMethodCommand("UpdateSubCategory", requestFields));
        }

        protected override DataActionResult DeleteItem(string[] keyValues)
        {
            return this.RequestFields("SubCategoryAction", WithMethodCommand("DeleteSubCategory", this.KeyFields(keyValues)));
        }
    }
}
