

SELECT 
--Distinct
p.CategoryID 
--c.ID, c.Name, c.ParentCategoryID
  FROM [dbo].[ProductShops] ps
  INNER JOIN dbo.[Products] p ON p.ID = ps.ProductID
 --INNER JOIN [dbo].[ShopCategories] sc ON sc.CategoryID = p.CategoryID
  --INNER JOIN dbo.Categories c ON p.CategoryID = c.ID

  WHERE ps.ShopID = 49



