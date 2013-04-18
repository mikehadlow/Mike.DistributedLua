using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace Mike.DistributedLua
{
    public class LuaTableToClrTypeMapper
    {
        private readonly Lua lua;

        public LuaTableToClrTypeMapper(Lua lua)
        {
            this.lua = lua;
        }

        public LuaTable ClrTypeToLuaTable(Type type, object instance)
        {
            var luaTable = CreateLuaTable();
            foreach (var propertyInfo in type.GetProperties())
            {
                luaTable[propertyInfo.Name] = 
                    ClrTypeToLuaValue(propertyInfo.PropertyType, propertyInfo.GetValue(instance));
            }
            return luaTable;
        }

        private object ClrTypeToLuaValue(Type type, object instance)
        {
            if (type == typeof(int))
            {
                return instance;
            }
            if (type == typeof (double))
            {
                return instance;
            }
            if (type == typeof(string))
            {
                return instance;
            }
            if (type == typeof(DateTime))
            {
                return ((DateTime)instance).ToString();
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    var luaArrayTable = CreateLuaTable();
                    var itemType = type.GenericTypeArguments[0];
                    var count = 0;
                    foreach (var item in (IEnumerable)instance)
                    {
                        luaArrayTable[count] = ClrTypeToLuaValue(itemType, item);
                        count++;
                    }
                    return luaArrayTable;
                }
                throw new ApplicationException("Non generic IEnumberable not allowed in DTO types");
            }
            return ClrTypeToLuaTable(type, instance);
        }

        public object LuaTableToClrType(Type type, LuaTable luaTable)
        {
            var instance = Activator.CreateInstance(type);
            foreach (var propertyInfo in type.GetProperties())
            {
                // Need to access basic types, or create instances of enumerables or 
                // user defined types.
                propertyInfo.SetValue(instance, LuaValueToClrType(propertyInfo.PropertyType, luaTable[propertyInfo.Name]));
            }
            return instance;
        }

        private object LuaValueToClrType(Type targetType, object luaValue)
        {
            if (luaValue == null)
            {
                throw new ArgumentNullException("luaValue");
            }

            if (luaValue is bool && targetType == typeof (bool))
            {
                return luaValue;
            }
            if(luaValue is double && targetType == typeof(int))
            {
                return Convert.ToInt32(luaValue);
            }
            if (luaValue is double && targetType == typeof (double))
            {
                return (double) luaValue;
            }
            if (luaValue is string && targetType == typeof(string))
            {
                return luaValue;
            }
            if (luaValue is string && targetType == typeof (DateTime))
            {
                return DateTime.Parse((string)luaValue);
            }

            var luaTable = luaValue as LuaTable;
            if (luaTable != null && typeof(IEnumerable).IsAssignableFrom(targetType))
            {
                return LuaTableToCollectionType(targetType, luaTable);
            }
            if (luaTable != null)
            {
                // process LuaTable as object
                return LuaTableToClrType(targetType, luaTable);
            }

            // no conversion found
            throw new ApplicationException(string.Format("Cannon convert luaType {0} to CLR type {1}",
                luaValue.GetType(), targetType));
        }

        private object LuaTableToCollectionType(Type targetType, LuaTable luaTable)
        {
            if (typeof (IDictionary).IsAssignableFrom(targetType))
            {
                // attempt to convert to dictionary type
                var dictionary = (IDictionary) Activator.CreateInstance(targetType);
                foreach (var key in luaTable.Keys)
                {
                    dictionary.Add(key, luaTable[key]);
                }
            }
            if (typeof (ICollection).IsAssignableFrom(targetType))
            {
                if (!targetType.IsGenericType)
                {
                    throw new ApplicationException(string.Format("Target type {0} is not a generic collection type.",
                        targetType));
                }

                var collectionItemType = targetType.GetGenericArguments()[0];

                var addMethod = targetType.GetMethod("Add");
                if (addMethod == null)
                {
                    throw new ApplicationException(
                        string.Format("Target type {0} is a collection without an Add method", targetType));
                }

                // attempt to convert to collection
                var collection = (ICollection)Activator.CreateInstance(targetType);
                foreach (var value in luaTable.Values)
                {
                    var clrValue = LuaValueToClrType(collectionItemType, value);
                    addMethod.Invoke(collection, new object[] {clrValue});
                }
                return collection;
            }

            throw new ApplicationException(string.Format("Cannon convert LuaTable to enumerable target type {0}",
                targetType.Name));
        }

        private LuaTable CreateLuaTable()
        {
            return (LuaTable)lua.DoString("return {}")[0];
        }
    }


    public class LuaTableToClrTypeMapperTests
    {
        public void ConversionTest()
        {
            using (var lua = new Lua())
            {
                var mapper = new LuaTableToClrTypeMapper(lua);

                var order = new Order
                    {
                        Id = "my_order",
                        Date = new DateTime(2012, 4, 10),
                        OrderLines =
                            {
                                new OrderLine
                                    {
                                        Product = "widget",
                                        Quantity = 2,
                                        Price = 12.35
                                    },
                                new OrderLine
                                    {
                                        Product = "Gadget",
                                        Quantity = 3,
                                        Price = 9.99
                                    },
                            }
                    };

                var luaTable = mapper.ClrTypeToLuaTable(typeof (Order), order);

                Console.Out.WriteLine(luaTable["Id"]);
                Console.Out.WriteLine(luaTable["Date"]);
                var orderLines = (LuaTable)luaTable["OrderLines"];
                foreach (DictionaryEntry orderLine in orderLines)
                {
                    var orderLineTable = (LuaTable) orderLine.Value;
                    Console.Out.WriteLine(orderLineTable["Product"]);
                    Console.Out.WriteLine(orderLineTable["Quantity"]);
                    Console.Out.WriteLine(orderLineTable["Price"]);
                }

                Console.Out.WriteLine("\nConverting back to CLR Order ...\n");

                var returnedOrder = (Order)mapper.LuaTableToClrType(typeof (Order), luaTable);

                Console.Out.WriteLine(returnedOrder.Id);
                Console.Out.WriteLine(returnedOrder.Date);
                foreach (var orderLine in returnedOrder.OrderLines)
                {
                    Console.Out.WriteLine(orderLine.Product);
                    Console.Out.WriteLine(orderLine.Quantity);
                    Console.Out.WriteLine(orderLine.Price);
                }
            }
        }
    }

    public class Order
    {
        public Order()
        {
            OrderLines = new List<OrderLine>();
        }

        public string Id { get; set; }
        public DateTime Date { get; set; }
        public List<OrderLine> OrderLines { get; set; }
    }

    public class OrderLine
    {
        public string Product { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}