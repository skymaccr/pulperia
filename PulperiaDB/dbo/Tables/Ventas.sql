CREATE TABLE [dbo].[Ventas] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [IdComprador] INT             NOT NULL,
    [IdProducto]  INT             NOT NULL,
    [Cantidad]    INT             NOT NULL,
    [Precio]      DECIMAL (18, 2) NOT NULL,
    [FechaCompra] DATETIME        NOT NULL,
    CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Ventas_Compradores] FOREIGN KEY ([IdComprador]) REFERENCES [dbo].[Compradores] ([Id]),
    CONSTRAINT [FK_Ventas_Productos] FOREIGN KEY ([IdProducto]) REFERENCES [dbo].[Productos] ([Id])
);

