
     
	 UPDATE cat 
   SET cat.[NotInCategory] = 0
FROM [ProductShops] cat

UPDATE cat 
   SET cat.[NotInCategory] = 1
      

FROM [ProductShops] cat
WHERE NOT EXISTS(
 SELECT 
 TOP 1 'c' as 'has' 
 FROM 
  [dbo].[Products] p 
  INNER JOIN [dbo].[ShopCategories] pcm
  ON pcm.[CategoryID] = p.CategoryID AND pcm.[ShopID] = cat.ShopID
 WHERE
 cat.ProductID = p.ID
 AND pcm.CategoryID > 0
AND  p.CategoryID IN 
( 
SELECT pcm.[CategoryID] 
UNION
SELECT ID FROM [dbo].[Categories] c WHERE c.[ParentCategoryID] =pcm.[CategoryID]
UNION 
SELECT ID FROM [dbo].[Categories] c2 WHERE c2.[ID]= ( SELECT [ParentCategoryID] FROM [dbo].[Categories] WHERE [ID] = pcm.[CategoryID])
	
)

 ) 
 AND NOT EXISTS (
 
  SELECT 
 TOP 1 'c' as 'has' 
 FROM 
  [dbo].[Products] p 
  INNER JOIN [dbo].[ShopCategoryMenus] pcm
  ON pcm.[CategoryID] = p.CategoryID AND pcm.[ShopID] = cat.ShopID
 WHERE
 cat.ProductID = p.ID
 AND pcm.CategoryID > 0



 )
 




  UPDATE cat 
   SET cat.[Published] = 1
      

FROM [ShopCategories] cat
WHERE EXISTS(
 SELECT 
 TOP 1 'c' as 'has' 
 FROM [dbo].[ProductShops] ps
 WHERE ps.[NotInCategory] = 1
) 
AND cat.CategoryID = (SELECT TOP 1 ID FROM [dbo].[Categories] c3 WHERE c3.[Name]= N'מוצרים נוספים' )

update ProductShops set SellCount = 
     (select COUNT(ID) from OrderItems where OrderItems.ProductShopID = ProductShops.ID)