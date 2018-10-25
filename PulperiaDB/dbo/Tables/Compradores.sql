CREATE TABLE [dbo].[Compradores] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [Nombre]     NVARCHAR (50)  NOT NULL,
    [Apellidos]  NVARCHAR (100) NOT NULL,
    [EsAsociado] BIT            NOT NULL,
    [Activo]     BIT            NOT NULL,
    [Email]      NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_Compradores] PRIMARY KEY CLUSTERED ([Id] ASC)
);

