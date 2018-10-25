-- =============================================
-- Author:		Ronny Muñoz
-- Create date: 10/8/2018
-- Description:	Ejecuta el Cierre de ventas mensual
-- =============================================
CREATE PROCEDURE [dbo].[SP_CierreMensual]
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [dbo].[HistoricoVentas]
           ([IdComprador]
           ,[IdProducto]
           ,[Cantidad]
           ,[Precio]
           ,[FechaCompra]
           ,[NombreComprador]
           ,[NombreProducto]
           ,[PrecioOriginalProducto]
           ,[Mes]
           ,[Anno])
     SELECT 
		cpr.Id AS IdComprador,
		pdt.Id AS IdProducto,
		vnt.Cantidad,
		vnt.Precio,
		vnt.FechaCompra,
		cpr.Nombre + ' '+ cpr.Apellidos AS NombreComprador,
		pdt.Nombre AS NombreProducto,
		pdt.PrecioCompraIndividual AS PrecioOriginalProducto,
		MONTH(GETDATE()) AS Mes,
		YEAR (GETDATE()) AS Anno
	FROM 
		[dbo].[Ventas] vnt inner join [dbo].[Productos] pdt
		on vnt.IdProducto = pdt.Id
		inner join [dbo].[Compradores] cpr
		on vnt.IdComprador = cpr.Id

	--una vez migrados los registros se elimina todo de la tabla de ventas
	TRUNCATE TABLE [dbo].[Ventas]
END