--- Product Image fix
update Products set HasImage = 1
where Image is not null and Image <> N'' and HasImage = 0

update Products set HasImage = 0
where (Image is  null or Image = N'') and HasImage = 1

--order products
Update ps Set ps.OrderPosition = ISNULL(( scm.GroupNumber * 1000 + scm.DisplayOrder),123456)

--select TOP 100 scm.GroupNumber * 1000 + scm.DisplayOrder as OrderPosition ,scm.*,ps.*
 from  ProductShops ps
left join Products p ON p.ID = ps.ProductID
left join ShopCategoryMenus scm ON scm.CategoryID = p.CategoryID AND ps.ShopID = scm.ShopID
--where ps.ShopID = 46

---- menu hide

      UPDATE cat 
   SET cat.[Published] = 1
   FROM [ShopCategoryMenus] cat

UPDATE cat 
   SET cat.[Published] = 0
      
 --select *
FROM [ShopCategoryMenus] cat
WHERE cat.[Level] = 1 AND NOT EXISTS(
 SELECT 
 TOP 1 'c' as 'has' 
 FROM [dbo].[ProductShops] ps

  INNER JOIN [dbo].[Products] p ON p.[ID] =  ps.[ProductID] AND ps.[ShopID] = cat.ShopID
   --INNER JOIN [dbo].[ShopCategories] pcm ON pcm.ShopID = cat.ShopID AND
   WHERE p.[CategoryID] 
   IN  (
    SELECT cat.[CategoryID] 
	UNION SELECT ID FROM [dbo].[Categories] c WHERE c.[ParentCategoryID]= cat.[CategoryID] 
	
	--UNION SELECT ID FROM [dbo].[Categories] c2 
	--WHERE c2.[ID]= 	( SELECT TOP 1 [ParentCategoryID] FROM [dbo].[Categories] WHERE [ID] = pcm.[CategoryID])
	 ) 
-- WHERE ps.[ShopID] = cat.ShopID
 --AND p.[Deleted] = 0 AND p.[Published] = 1
 ) 

--- category hide
     

      UPDATE cat 
   SET cat.[Published] = 1
   FROM [ShopCategories] cat

UPDATE cat 
   SET cat.[Published] = 0
      
 --select *
FROM [ShopCategories] cat
WHERE NOT EXISTS(
 SELECT 
 TOP 1 'c' as 'has' 
 FROM [dbo].[ProductShops] ps

  INNER JOIN [dbo].[Products] p ON p.[ID] =  ps.[ProductID] AND ps.[ShopID] = cat.ShopID
   --INNER JOIN [dbo].[ShopCategories] pcm ON pcm.ShopID = cat.ShopID AND
   WHERE p.[CategoryID] 
   IN  (
    SELECT cat.[CategoryID] 
	UNION SELECT ID FROM [dbo].[Categories] c WHERE c.[ParentCategoryID]= cat.[CategoryID] 
	
	--UNION SELECT ID FROM [dbo].[Categories] c2 
	--WHERE c2.[ID]= 	( SELECT TOP 1 [ParentCategoryID] FROM [dbo].[Categories] WHERE [ID] = pcm.[CategoryID])
	 ) 
-- WHERE ps.[ShopID] = cat.ShopID
 --AND p.[Deleted] = 0 AND p.[Published] = 1
 ) 
 

 -- select * from ShopCategories

