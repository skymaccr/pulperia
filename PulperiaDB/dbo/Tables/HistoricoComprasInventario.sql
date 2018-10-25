CREATE TABLE [dbo].[HistoricoComprasInventario] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [IdProducto]         INT             NOT NULL,
    [NombreProducto]     NVARCHAR (256)  NOT NULL,
    [CantidadInventario] INT             NOT NULL,
    [CantidadComprada]   INT             NOT NULL,
    [FechaCompra]        DATETIME        NOT NULL,
    [FechaVencimiento]   DATETIME        NOT NULL,
    [Lote]               NVARCHAR (256)  NOT NULL,
    [PrecioInvividual]   DECIMAL (18, 2) NOT NULL,
    [Mes]                INT             NOT NULL,
    [Anno]               INT             NOT NULL,
    CONSTRAINT [PK_HistoricoComprasInventario] PRIMARY KEY CLUSTERED ([Id] ASC)
);

