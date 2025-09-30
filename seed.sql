-- ============================
-- RESETAR DADOS (opcional, só se precisa começar limpo)
-- ============================
DELETE FROM PesquisasCustomizadas;
DELETE FROM Artigos;
DELETE FROM Autores;
DELETE FROM Fontes;
DELETE FROM Categorias;
DELETE FROM Usuarios;

-- ============================
-- POPULAR USUÁRIOS
-- ============================
INSERT INTO Usuarios (Nome, Email, SenhaHash, Role, Ativo, DataCadastro) VALUES
('Admin', 'admin@email.com', '123456', 'Admin', 1, CURRENT_TIMESTAMP),
('João Silva', 'joao.user@email.com', '123456', 'User', 1, CURRENT_TIMESTAMP),
('Maria Oliveira', 'maria.user@email.com', '123456', 'User', 1, CURRENT_TIMESTAMP),
('Carlos Mendes', 'carlos.user@email.com', '123456', 'User', 1, CURRENT_TIMESTAMP),
('Ana Costa', 'ana.user@email.com', '123456', 'User', 1, CURRENT_TIMESTAMP);

-- ============================
-- POPULAR CATEGORIAS
-- ============================
INSERT INTO Categorias (Nome, Slug, Descricao, Ativo) VALUES
('Tecnologia', 'tecnologia', 'Notícias sobre tecnologia e inovação', 1),
('Educação', 'educacao', 'Artigos voltados à área da educação', 1),
('Ciência', 'ciencia', 'Descobertas e avanços científicos', 1),
('Esportes', 'esportes', 'Esportes nacionais e internacionais', 1),
('Saúde', 'saude', 'Notícias sobre saúde e bem-estar', 1);

-- ============================
-- POPULAR FONTES
-- ============================
INSERT INTO Fontes (Nome, URL, Tipo, Ativo) VALUES
('Agência Angola Press', 'https://www.angop.ao', 'Agência', 1),
('BBC', 'https://www.bbc.com', 'Externa', 1),
('CNN', 'https://www.cnn.com', 'Internacional', 1),
('The Guardian', 'https://www.theguardian.com', 'Externa', 1),
('DW África', 'https://www.dw.com', 'Internacional', 1);

-- ============================
-- POPULAR AUTORES
-- ============================
INSERT INTO Autores (Nome, Email, Bio, Ativo) VALUES
('João Silva', 'autor.joao@email.com', 'Jornalista especializado em tecnologia.', 1),
('Maria Oliveira', 'autor.maria@email.com', 'Pesquisadora na área de educação.', 1),
('Carlos Mendes', 'autor.carlos@email.com', 'Divulgador científico e palestrante.', 1),
('Ana Costa', 'autor.ana@email.com', 'Escritora na área de saúde.', 1),
('Pedro Gomes', 'autor.pedro@email.com', 'Repórter esportivo.', 1);

-- ============================
-- POPULAR ARTIGOS
-- ============================
INSERT INTO Artigos 
(Titulo, Slug, Conteudo, Resumo, CategoriaId, AutorId, FonteId, IsPublicado, DataCriacao) VALUES
('Primeiro Artigo', 'primeiro-artigo', 'Conteúdo sobre inovação tecnológica.', 'Resumo do artigo 1', 1, 1, 1, 1, CURRENT_TIMESTAMP),
('Educação no Século XXI', 'educacao-seculo-xxi', 'Discussão sobre métodos educacionais modernos.', 'Resumo do artigo 2', 2, 2, 2, 1, CURRENT_TIMESTAMP),
('Ciência Moderna', 'ciencia-moderna', 'Texto sobre avanços científicos.', 'Resumo do artigo 3', 3, 3, 3, 1, CURRENT_TIMESTAMP),
('Esportes em Angola', 'esportes-angola', 'Cobertura esportiva nacional.', 'Resumo do artigo 4', 4, 5, 4, 1, CURRENT_TIMESTAMP),
('Saúde Pública', 'saude-publica', 'Análise da saúde em Angola.', 'Resumo do artigo 5', 5, 4, 5, 1, CURRENT_TIMESTAMP);

-- ============================
-- POPULAR PESQUISA CUSTOMIZADA
-- Cada usuário escolhe preferências de Categoria, Autor e Fonte
-- ============================
INSERT INTO PesquisasCustomizadas (UsuarioId, CategoriaId, AutorId, FonteId, DataCriacao) VALUES
(2, 1, 1, 1, CURRENT_TIMESTAMP), -- João Silva gosta de tecnologia, autor João Silva, fonte Angop
(2, 3, 3, 3, CURRENT_TIMESTAMP), -- João também segue ciência (Carlos Mendes, CNN)
(3, 2, 2, 2, CURRENT_TIMESTAMP), -- Maria prefere educação, autora Maria, fonte BBC
(4, 5, 4, 5, CURRENT_TIMESTAMP), -- Carlos segue saúde (Ana Costa, DW África)
(5, 4, 5, 4, CURRENT_TIMESTAMP); -- Ana gosta de esportes (Pedro Gomes, The Guardian)
