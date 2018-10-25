CREATE TABLE [dbo].[Productos] (
    [Id]                     INT             IDENTITY (1, 1) NOT NULL,
    [Nombre]                 NVARCHAR (256)  NOT NULL,
    [Descripcion]            NVARCHAR (256)  NOT NULL,
    [FechaIngreso]           DATETIME        NOT NULL,
    [CantidadInventario]     INT             NOT NULL,
    [PrecioCompraIndividual] DECIMAL (18, 2) NOT NULL,
    [IdCategoria]            INT             NOT NULL,
    [FechaCreacion]          DATETIME        NOT NULL,
    [FechaActualizacion]     DATETIME        NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Productos_Categorias] FOREIGN KEY ([IdCategoria]) REFERENCES [dbo].[Categorias] ([Id])
);



