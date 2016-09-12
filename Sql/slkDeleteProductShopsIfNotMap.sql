--DELETE FROM ProductShops where ID IN
(
select  ps.ID from ProductShops ps
Left join Products p ON p.ID = ps.ProductID
--where p.ID IS NULL
)