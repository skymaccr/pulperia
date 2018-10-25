CREATE TABLE [dbo].[HistoricoVentas] (
    [Id]                     INT             IDENTITY (1, 1) NOT NULL,
    [IdComprador]            INT             NOT NULL,
    [IdProducto]             INT             NOT NULL,
    [Cantidad]               INT             NOT NULL,
    [Precio]                 DECIMAL (18, 2) NOT NULL,
    [FechaCompra]            DATETIME        NOT NULL,
    [NombreComprador]        NVARCHAR (256)  NOT NULL,
    [NombreProducto]         NVARCHAR (256)  NOT NULL,
    [PrecioOriginalProducto] DECIMAL (18, 2) NOT NULL,
    [Mes]                    INT             NOT NULL,
    [Anno]                   INT             NOT NULL,
    CONSTRAINT [PK_HistoricoVentas] PRIMARY KEY CLUSTERED ([Id] ASC)
);

