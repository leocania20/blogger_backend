
-- =========================================
-- 🚀 POPULAR TABELAS BASE (Render / PostgreSQL)
-- =========================================

SET search_path TO public;


-- 1️⃣ Inserir Categorias
INSERT INTO "Categories" ("Id", "Name", "Description" ,"Tag", "Active") VALUES
(1, 'Tecnologia', 'Notícias sobre inovações tecnológicas', 'tech', true),
(2, 'Economia', 'Tópicos sobre o mercado financeiro e negócios', 'economy', true),
(3, 'Ciência', 'Descobertas e estudos científicos', 'science', true),
(4, 'Educação', 'Artigos sobre ensino e aprendizagem', 'education', true),
(5, 'Saúde', 'Avanços médicos e bem-estar', 'health', true),
(6, 'Ambiente', 'Sustentabilidade e meio ambiente', 'environment', true),
(7, 'Desporto', 'Eventos e análises desportivas', 'sports', true),
(8, 'Política', 'Notícias sobre políticas locais e internacionais', 'politics', true),
(9, 'Cultura', 'Artes, literatura e cinema', 'culture', true),
(10, 'Tecnologia da Informação', 'Soluções digitais e sistemas', 'it', true);

-- 2️⃣ Inserir Fontes (Sources)
INSERT INTO "Sources" ("Id", "Name", "URL", "Type", "Active") VALUES
(1, 'BBC News', 'https://www.bbc.com', 'Internacional', true),
(2, 'CNN', 'https://www.cnn.com', 'Internacional', true),
(3, 'Expansão', 'https://expansao.co.ao', 'Nacional', true),
(4, 'AngoNoticias', 'https://angonoticias.com', 'Nacional', true),
(5, 'TechRadar', 'https://www.techradar.com', 'Tecnologia', true),
(6, 'Nature', 'https://www.nature.com', 'Ciência', true),
(7, 'Jornal de Angola', 'https://www.jornaldeangola.ao', 'Nacional', true),
(8, 'Medium', 'https://medium.com', 'Blog', true),
(9, 'The Guardian', 'https://www.theguardian.com', 'Internacional', true),
(10, 'Google News', 'https://news.google.com', 'Agregador', true);

-- 3️⃣ Inserir Autores
INSERT INTO "Authores" ("Id", "Name", "Bio", "Email", "UserId", "Active") VALUES
(1, 'Leocânia Melo', 'Engenheiro Informático apaixonado por IA e automação.', 'leocania@blogger.com',1, true),
(2, 'Avelino Silvano', 'Jornalista com foco em tecnologia e inovação.', 'avelino@blogger.com', 2, true),
(3, 'Maria António', 'Especialista em economia e finanças.', 'maria@blogger.com', 3, true),
(4, 'Carlos Tavares', 'Pesquisador em ciências ambientais.', 'carlos@blogger.com',4,  true),
(5, 'Ana Luísa', 'Escritora e analista de cultura.', 'ana@blogger.com', 5, true);

-- 4️⃣ Inserir Pesquisas Personalizadas
INSERT INTO "CustomizedResearches" ("Id", "UserId", "CategoryId", "AuthorId", "SourceId", "CreateDate") VALUES
(1, 1, 1, 2, 5, NOW()),
(2, 1, 3, 4, 6, NOW()),
(3, 2, 2, 3, 3, NOW()),
(4, 2, 5, 1, 8, NOW()),
(5, 3, 9, 5, 9, NOW()),
(6, 3, 10, 2, 5, NOW()),
(7, 4, 4, 3, 2, NOW()),
(8, 4, 7, 1, 4, NOW()),
(9, 5, 8, 4, 7, NOW()),
(10, 5, 6, 2, 10, NOW());

-- 5️⃣ Inserir Artigos
INSERT INTO "Articles" ("Id", "Title", "Tag", "Text", "Summary", "CreateDate", "UpDate", "PublishedDate", "IsPublished", "Imagem", "CategoryId", "AuthorId", "SourceId")
VALUES
(1, 'A Revolução da IA na Educação', 'ai-edu', 'A inteligência artificial está transformando o ensino em todo o mundo...', 'Como a IA está mudando a educação.', NOW(), NULL, NOW(), true, 'https://blogger-backend-6.onrender.com//uploads/artigos/5bd876af1daad6b3965689200d2f9f55d68ec739.jpg', 4, 1, 5),
(2, 'Mercado Financeiro Angolano em Alta', 'financas', 'O mercado angolano mostra sinais de crescimento sustentável...', 'Perspectivas otimistas na economia de Angola.', NOW(), NULL, NOW(), true, 'https://blogger-backend-6.onrender.com//uploads/artigos/Gemini_Generated_Image_l8v29hl8v29hl8v2.png', 2, 3, 3),
(3, 'Novo Avanço em Energia Solar', 'energia-solar', 'Pesquisadores criam painéis 30% mais eficientes...', 'Energia limpa em destaque.', NOW(), NULL, NOW(), true, 'energia_solar.jpg', 6, 4, 6),
(4, 'O Futuro do Jornalismo Digital', 'jornalismo', 'O jornalismo enfrenta novos desafios com a era digital...', 'O impacto da tecnologia na mídia.', NOW(), NULL, NOW(), true, 'https://blogger-backend-6.onrender.com//uploads/artigos/Gemini_Generated_Image_l8v29hl8v29hl8v2.png', 1, 2, 9),
(5, 'Saúde Mental e Trabalho Remoto', 'saude-trabalho', 'Trabalhar em casa exige novos cuidados com a saúde mental...', 'Equilíbrio entre produtividade e bem-estar.', NOW(), NULL, NOW(), true, 'https://blogger-backend-6.onrender.com//uploads/artigos/Gemini_Generated_Image_l8v29hl8v29hl8v2.png', 5, 5, 4),
(6, 'Os 10 Melhores Gadgets de 2025', 'gadgets2025', 'Listamos os gadgets mais inovadores deste ano...', 'O futuro na palma da mão.', NOW(), NULL, NOW(), true, 'gadgets2025.jpg', 10, 1, 5),
(7, 'Crescimento das Startups Africanas', 'startups', 'Empreendedores africanos ganham espaço globalmente...', 'O continente como novo polo tecnológico.', NOW(), NULL, NOW(), true, 'https://blogger-backend-6.onrender.com//uploads/artigos/d0a39c40120c2c0e4754d627c83b9333f7aa2a74.jpg', 2, 2, 8),
(8, 'Descoberta de Nova Partícula Subatômica', 'fisica', 'Cientistas anunciam uma descoberta que pode mudar a física...', 'Um novo capítulo na ciência.', NOW(), NULL, NOW(), true, 'https://blogger-backend-6.onrender.com//uploads/artigos/5bd876af1daad6b3965689200d2f9f55d68ec739.jpg', 3, 4, 6),
(9, 'O Papel da Mulher na Tecnologia', 'mulher-tech', 'Cada vez mais mulheres se destacam no setor tecnológico...', 'Inclusão e inovação.', NOW(), NULL, NOW(), true, 'https://blogger-backend-6.onrender.com//uploads/artigos/d0a39c40120c2c0e4754d627c83b9333f7aa2a74.jpg', 1, 5, 8),
(10, 'Mudanças Climáticas e Energia Sustentável', 'clima', 'Como o aquecimento global afeta o futuro da energia...', 'Soluções verdes em pauta.', NOW(), NULL, NOW(), true, 'https://blogger-backend-6.onrender.com//uploads/artigos/d0a39c40120c2c0e4754d627c83b9333f7aa2a74.jpg', 6, 4, 7);


-- ==============================
-- 🚀 Função: cria notificações automáticas ao cadastrar artigo
-- ==============================

CREATE OR REPLACE FUNCTION fn_notify_users_on_new_article()
RETURNS TRIGGER AS $$
BEGIN
    -- Apenas cria notificação se o artigo estiver publicado
    IF NEW."IsPublished" = TRUE THEN
        INSERT INTO "Notifications" (
            "Title",
            "Message",
            "Type",
            "Readed",
            "CreateDate",
            "Active",
            "UserId",
            "ArticleId"
        )
        SELECT DISTINCT
            'Novo artigo publicado: ' || NEW."Title" AS "Title",
            'Um novo artigo relacionado às suas preferências foi publicado.' AS "Message",
            'Artigo' AS "Type",
            FALSE AS "Readed",
            NOW() AS "CreateDate",
            TRUE AS "Active",
            cr."UserId",
            NEW."Id"
        FROM "CustomizedResearches" cr
        WHERE
            (cr."CategoryId" IS NOT NULL AND cr."CategoryId" = NEW."CategoryId")
            OR (cr."AuthorId" IS NOT NULL AND cr."AuthorId" = NEW."AuthorId")
            OR (cr."SourceId" IS NOT NULL AND cr."SourceId" = NEW."SourceId")
        -- evita notificações duplicadas
        AND NOT EXISTS (
            SELECT 1 FROM "Notifications" n
            WHERE n."UserId" = cr."UserId"
              AND n."ArticleId" = NEW."Id"
        );
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE TRIGGER trg_notify_on_article_insert
AFTER INSERT ON "Articles"
FOR EACH ROW
EXECUTE FUNCTION fn_notify_users_on_new_article();
