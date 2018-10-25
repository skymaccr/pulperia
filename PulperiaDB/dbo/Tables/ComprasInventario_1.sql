CREATE TABLE [dbo].[ComprasInventario] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [IdProducto]       INT             NULL,
    [CantidadComprada] INT             NULL,
    [FechaCompra]      DATETIME        NULL,
    [FechaVencimiento] DATETIME        NULL,
    [Lote]             NVARCHAR (256)  NULL,
    [PrecioIndividual] DECIMAL (18, 2) NULL,
    CONSTRAINT [PK_ComprasInventario] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ComprasInventario_Productos] FOREIGN KEY ([IdProducto]) REFERENCES [dbo].[Productos] ([Id])
);

