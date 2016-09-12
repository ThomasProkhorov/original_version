DROP table #TempSku

SELECT  [ID]
      ,[SKU]
	  INTO #TempSku
	  FROM [dbo].[Products] p
WHERE LEN(p.SKU) < 10 






SELECT  [ID]
      ,[SKU]
	  
	  ,(SELECT TOP 1 ID
	  --Cast(ID as nvarchar) +', ' +SKU + ', '+Name
  FROM [dbo].[Products] p2 
	  WHERE  LEN(p2.SKU) > 11 AND ( PATINDEX(N'%'+p.SKU,p2.SKU) > 0  OR PATINDEX(p.SKU+N'%',p2.SKU) > 0)
	  AND p.ID <> p2.ID) as skulost
     
	 
	  ,[Name]
      ,[ShortDescription]
      ,[FullDescription]
    --  ,[Image]
     
      ,[Deleted]
  FROM [dbo].[Products] p
WHERE LEN(p.SKU) < 10 


