-- =============================================
-- Author:		Ronny Muñoz Nuñez
-- Create date: 10/9/2018
-- Description:	Ejecuta un ajuste de inventario
-- =============================================
CREATE PROCEDURE [dbo].[SP_AjusteInventario] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--pasar el ajuste de inventario al historico
	INSERT INTO [dbo].[HistoricoComprasInventario]
           ([IdProducto]
           ,[NombreProducto]
           ,[CantidadInventario]
           ,[CantidadComprada]
           ,[FechaCompra]
           ,[FechaVencimiento]
           ,[Lote]
           ,[PrecioInvividual]
           ,[Mes]
           ,[Anno])
	SELECT
		  pro.Id,
		  pro.Nombre,
		  pro.CantidadInventario,
		  cin.CantidadComprada,
		  cin.FechaCompra,
		  cin.FechaVencimiento,
		  cin.Lote,
		  cin.PrecioIndividual,
		  MONTH(GETDATE()),
		  YEAR(GETDATE())
	FROM	
		ComprasInventario cin inner join Productos pro
		on cin.IdProducto = pro.Id

	--ajustar la cantidad de producto y el precio segun el ajuste de inventario
	UPDATE [dbo].[Productos]
	SET 
		[CantidadInventario] = inv.CantidadInventario,	
		[PrecioCompraIndividual] = inv.Precio
	FROM (SELECT	 
			(pro.CantidadInventario + cin.CantidadComprada) AS CantidadInventario,
			pro.Id	 AS IdProducto,
			--Mantener el precio mayor del producto. El producto no baja de precio a menos de que se edite en el producto
			CASE WHEN pro.PrecioCompraIndividual < cin.PrecioIndividual THEN cin.PrecioIndividual
			ELSE pro.PrecioCompraIndividual END AS Precio
		  FROM	
			ComprasInventario cin inner join Productos pro
			on cin.IdProducto = pro.Id ) as inv
	WHERE Id = inv.IdProducto

	--eliminar las compras de inventario
	TRUNCATE TABLE ComprasInventario

END