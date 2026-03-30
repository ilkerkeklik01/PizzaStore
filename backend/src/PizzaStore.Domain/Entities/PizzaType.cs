namespace PizzaStore.Domain.Entities;

/// <summary>
/// Pizza categories and specialty types available in the store
/// </summary>
public enum PizzaType
{
    /// <summary>
    /// Vegetarian pizza with no meat toppings
    /// </summary>
    Vegetarian,
    
    /// <summary>
    /// Pizza loaded with various meat toppings like pepperoni, sausage, and bacon
    /// </summary>
    MeatLovers,
    
    /// <summary>
    /// Classic Hawaiian pizza with ham and pineapple
    /// </summary>
    Hawaiian,
    
    /// <summary>
    /// Pizza with a variety of fresh vegetables
    /// </summary>
    Veggie,
    
    /// <summary>
    /// Build-your-own pizza with custom toppings chosen by the customer
    /// </summary>
    Custom,
    
    /// <summary>
    /// Loaded pizza with a combination of meat and vegetable toppings
    /// </summary>
    Supreme,
    
    /// <summary>
    /// Traditional Italian pizza with tomato sauce, mozzarella, and basil
    /// </summary>
    Margherita
}
