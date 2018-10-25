CREATE TABLE [dbo].[Parametros] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Nombre]      NVARCHAR (256) NOT NULL,
    [Valor]       NVARCHAR (256) NOT NULL,
    [FechaInicio] DATETIME       NOT NULL,
    [FechaFin]    DATETIME       NULL,
    CONSTRAINT [PK_Parametros] PRIMARY KEY CLUSTERED ([Id] ASC)
);

