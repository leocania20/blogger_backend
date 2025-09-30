-- Corrigir os artigos para usar Categoria, Fonte e Autor corretos
UPDATE Artigos SET CategoriaId = 21, FonteId = 21, AutorId = 21 WHERE Id = 26;
UPDATE Artigos SET CategoriaId = 22, FonteId = 22, AutorId = 22 WHERE Id = 27;
UPDATE Artigos SET CategoriaId = 23, FonteId = 23, AutorId = 23 WHERE Id = 28;
UPDATE Artigos SET CategoriaId = 24, FonteId = 24, AutorId = 24 WHERE Id = 29;
UPDATE Artigos SET CategoriaId = 25, FonteId = 25, AutorId = 25 WHERE Id = 30;

-- (Opcional) Ajustar DataCriacao para testes de pesquisa
UPDATE Artigos SET DataCriacao = '2025-02-05' WHERE Id = 26;
UPDATE Artigos SET DataCriacao = '2025-02-05' WHERE Id = 27;
UPDATE Artigos SET DataCriacao = '2025-09-01' WHERE Id = 28;
UPDATE Artigos SET DataCriacao = '2025-09-15' WHERE Id = 29;
UPDATE Artigos SET DataCriacao = '2025-09-29' WHERE Id = 30;
