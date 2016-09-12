--- category hide
    
	 UPDATE cat 
   SET cat.[HaveDiscount] = 0
FROM [ProductShops] cat

UPDATE cat 
   SET cat.[HaveDiscount] = 1
      
 --select *
FROM [ProductShops] cat
WHERE  EXISTS(
 SELECT ID FROM [Discounts] dis
	 WHERE PATINDEX('%,'+CAST( cat.ID as nvarchar)+',%',dis.ProductShopIDs) > 0
	 AND (dis.StartDate IS NULL OR dis.StartDate < GETDATE ())
	  AND (dis.EndDate IS NULL OR dis.EndDate > GETDATE ())
)
 --AND p.[Deleted] = 0 AND p.[Published] = 1
 
 

  --select * from ShopCategories
 --  select * from ProductShops