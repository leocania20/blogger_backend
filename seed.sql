BEGIN;

-- ============================
-- POPULAR CATEGORIAS
-- ============================
INSERT INTO "Categorias" ("Nome", "Descricao", "Slug", "Ativo") VALUES
('Tecnologia', 'Notícias sobre tecnologia e inovação', 'tecnologia', TRUE),
('Educação', 'Artigos voltados à área da educação', 'educacao', TRUE),
('Ciência', 'Descobertas e avanços científicos', 'ciencia', TRUE),
('Esportes', 'Esportes nacionais e internacionais', 'esportes', TRUE),
('Saúde', 'Notícias sobre saúde e bem-estar', 'saude', TRUE);

-- ============================
-- POPULAR FONTES
-- ============================
INSERT INTO "Fontes" ("Nome", "URL", "Tipo", "Ativo") VALUES
('Agência Angola Press', 'https://www.angop.ao', 'Agência', TRUE),
('BBC', 'https://www.bbc.com', 'Externa', TRUE),
('CNN', 'https://www.cnn.com', 'Internacional', TRUE),
('The Guardian', 'https://www.theguardian.com', 'Externa', TRUE),
('DW África', 'https://www.dw.com', 'Internacional', TRUE);

-- ============================
-- POPULAR AUTORES
-- ============================
INSERT INTO "Autores" ("Nome", "Email", "Bio", "Ativo") VALUES
('João Silva', 'autor.joao@email.com', 'Jornalista especializado em tecnologia.', TRUE),
('Maria Oliveira', 'autor.maria@email.com', 'Pesquisadora na área de educação.', TRUE),
('Carlos Mendes', 'autor.carlos@email.com', 'Divulgador científico e palestrante.', TRUE),
('Ana Costa', 'autor.ana@email.com', 'Escritora na área de saúde.', TRUE),
('Pedro Gomes', 'autor.pedro@email.com', 'Repórter esportivo.', TRUE);

-- ============================
-- POPULAR ARTIGOS (10)
-- ============================
INSERT INTO "Artigos" 
("Titulo", "Slug", "Conteudo", "Resumo", "CategoriaId", "AutorId", "FonteId", "IsPublicado", "DataCriacao", "Imagem") VALUES
('The Ultimate Guide to Bali: What to See, Do, and Eat', 'ultimate-guide-bali',
'It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout...',
'Descubra o melhor de Bali: praias, templos, e gastronomia incrível.', 1, 1, 1, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('Explorando o Futuro da Tecnologia', 'futuro-da-tecnologia',
'It is a long established fact that a reader will be distracted by the readable content...',
'Como a IA e a robótica estão moldando o futuro.', 1, 1, 2, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('Métodos Educacionais do Século XXI', 'metodos-educacionais-seculo-xxi',
'It is a long established fact that a reader will be distracted by the readable content...',
'Uma análise das novas tendências na educação.', 2, 2, 2, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('Descobertas Científicas Recentes', 'descobertas-cientificas',
'It is a long established fact that a reader will be distracted by the readable content...',
'Os mais recentes avanços no mundo da ciência.', 3, 3, 3, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('Saúde e Bem-Estar em Foco', 'saude-e-bem-estar',
'It is a long established fact that a reader will be distracted by the readable content...',
'Cuidados e práticas para uma vida saudável.', 5, 4, 5, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('Os Desafios do Jornalismo Digital', 'jornalismo-digital',
'It is a long established fact that a reader will be distracted by the readable content...',
'Como o jornalismo se adapta à era da informação.', 1, 1, 2, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('Educação Inclusiva no Século XXI', 'educacao-inclusiva',
'It is a long established fact that a reader will be distracted by the readable content...',
'Um olhar sobre a importância da inclusão nas escolas.', 2, 2, 2, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('Explorando o Universo', 'explorando-o-universo',
'It is a long established fact that a reader will be distracted by the readable content...',
'Uma viagem pelas galáxias e novas descobertas.', 3, 3, 3, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('Esportes e Motivação', 'esportes-e-motivacao',
'It is a long established fact that a reader will be distracted by the readable content...',
'A influência do esporte na saúde mental.', 4, 5, 4, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png'),
('A Importância da Alimentação Saudável', 'alimentacao-saudavel',
'It is a long established fact that a reader will be distracted by the readable content...',
'Como a alimentação afeta o corpo e a mente.', 5, 4, 5, TRUE, CURRENT_TIMESTAMP, '/uploads/artigos/Gemini_Generated_Image_ep4zydep4zydep4z.png');

-- ============================
-- POPULAR PESQUISAS CUSTOMIZADAS
-- ============================
INSERT INTO "PesquisasCustomizadas" ("UsuarioId", "CategoriaId", "AutorId", "FonteId", "DataCriacao") VALUES
(1, 1, 1, 1, CURRENT_TIMESTAMP),
(1, 3, 3, 3, CURRENT_TIMESTAMP),
(1, 2, 2, 2, CURRENT_TIMESTAMP),
(1, 5, 4, 5, CURRENT_TIMESTAMP),
(1, 4, 5, 4, CURRENT_TIMESTAMP);

COMMIT;
