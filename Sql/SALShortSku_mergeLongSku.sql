
SELECT 
p1.[ID]
      ,p1.[SKU]
      ,p1.[Name]
     ,p2.[ID]
      ,p2.[SKU]
      ,p2.[Name]
  FROM [dbo].[Products] p1
  INNER JOIN [dbo].[Products] p2
  ON LEN( p1.SKU) > LEN (p2.SKU)+ 4
  AND LEN (p2.SKU) < 11
  AND LEN (p2.SKU) > 5
  AND p1.SKU LIKE N'%'+p2.SKU



