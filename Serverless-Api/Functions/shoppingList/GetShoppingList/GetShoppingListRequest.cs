namespace Serverless_Api.Functions.ShoppingList.GetShoppingList
{
    public class GetShoppingListRequest
    {   
        public string Id { get; set; }
        public Dictionary<string, int> ShoppingList {  get; set; }
    }
}
