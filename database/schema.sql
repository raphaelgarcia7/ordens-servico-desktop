CREATE DATABASE gestao_os;

-- Execute os comandos abaixo conectado ao banco gestao_os.

CREATE TABLE clientes (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    documento VARCHAR(30) NOT NULL,
    tipo INTEGER NOT NULL,
    email VARCHAR(150),
    telefone VARCHAR(30),
    data_cadastro TIMESTAMP NOT NULL DEFAULT NOW(),
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_clientes_documento UNIQUE (documento),
    CONSTRAINT ck_clientes_tipo CHECK (tipo IN (1, 2))
);

CREATE TABLE servicos (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    valor_base NUMERIC(14, 2) NOT NULL,
    percentual_imposto NUMERIC(5, 2) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT ck_servicos_valor_base CHECK (valor_base > 0),
    CONSTRAINT ck_servicos_percentual_imposto CHECK (percentual_imposto >= 0 AND percentual_imposto <= 100)
);

CREATE TABLE ordens_servico (
    id SERIAL PRIMARY KEY,
    cliente_id INTEGER NOT NULL,
    data_abertura TIMESTAMP NOT NULL DEFAULT NOW(),
    data_conclusao TIMESTAMP NULL,
    status INTEGER NOT NULL,
    observacao TEXT,
    valor_total NUMERIC(14, 2) NOT NULL DEFAULT 0,
    versao INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT fk_ordens_servico_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id),
    CONSTRAINT ck_ordens_servico_status CHECK (status IN (1, 2, 3, 4)),
    CONSTRAINT ck_ordens_servico_valor_total CHECK (valor_total >= 0),
    CONSTRAINT ck_ordens_servico_versao CHECK (versao > 0)
);

CREATE TABLE ordem_servico_itens (
    id SERIAL PRIMARY KEY,
    ordem_servico_id INTEGER NOT NULL,
    servico_id INTEGER NOT NULL,
    quantidade INTEGER NOT NULL,
    valor_unitario NUMERIC(14, 2) NOT NULL,
    percentual_imposto_aplicado NUMERIC(5, 2) NOT NULL,
    valor_total_item NUMERIC(14, 2) NOT NULL,
    CONSTRAINT fk_os_itens_os FOREIGN KEY (ordem_servico_id) REFERENCES ordens_servico(id) ON DELETE CASCADE,
    CONSTRAINT fk_os_itens_servico FOREIGN KEY (servico_id) REFERENCES servicos(id),
    CONSTRAINT ck_os_itens_quantidade CHECK (quantidade > 0),
    CONSTRAINT ck_os_itens_valor_unitario CHECK (valor_unitario > 0),
    CONSTRAINT ck_os_itens_percentual CHECK (percentual_imposto_aplicado >= 0 AND percentual_imposto_aplicado <= 100),
    CONSTRAINT ck_os_itens_total CHECK (valor_total_item >= 0)
);

CREATE TABLE historico_status (
    id SERIAL PRIMARY KEY,
    ordem_servico_id INTEGER NOT NULL,
    status_anterior INTEGER NOT NULL,
    status_novo INTEGER NOT NULL,
    data_hora TIMESTAMP NOT NULL DEFAULT NOW(),
    usuario VARCHAR(120) NOT NULL,
    CONSTRAINT fk_historico_status_os FOREIGN KEY (ordem_servico_id) REFERENCES ordens_servico(id) ON DELETE CASCADE,
    CONSTRAINT ck_historico_status_anterior CHECK (status_anterior IN (1, 2, 3, 4)),
    CONSTRAINT ck_historico_status_novo CHECK (status_novo IN (1, 2, 3, 4))
);

CREATE TABLE auditoria (
    id SERIAL PRIMARY KEY,
    entidade VARCHAR(80) NOT NULL,
    id_registro INTEGER NOT NULL,
    operacao VARCHAR(30) NOT NULL,
    data_hora TIMESTAMP NOT NULL DEFAULT NOW(),
    usuario VARCHAR(120) NOT NULL,
    snapshot_json JSONB NOT NULL
);

CREATE INDEX ix_clientes_documento ON clientes(documento);
CREATE INDEX ix_ordens_servico_data_abertura ON ordens_servico(data_abertura);
CREATE INDEX ix_ordens_servico_status ON ordens_servico(status);
CREATE INDEX ix_ordens_servico_cliente_id ON ordens_servico(cliente_id);
CREATE INDEX ix_os_itens_ordem_servico_id ON ordem_servico_itens(ordem_servico_id);
CREATE INDEX ix_auditoria_entidade_registro ON auditoria(entidade, id_registro);

CREATE INDEX ix_clientes_ativos ON clientes(id) WHERE ativo = TRUE;
CREATE INDEX ix_servicos_ativos ON servicos(id) WHERE ativo = TRUE;

INSERT INTO clientes (nome, documento, tipo, email, telefone, ativo)
VALUES
('Cliente Pessoa Fisica', '12345678901', 1, 'pf@exemplo.com', '(11) 99999-0000', TRUE),
('Empresa Exemplo LTDA', '12345678000190', 2, 'contato@empresa.com', '(11) 3333-0000', TRUE);

INSERT INTO servicos (nome, valor_base, percentual_imposto, ativo)
VALUES
('Diagnostico tecnico', 150.00, 5.00, TRUE),
('Manutencao preventiva', 350.00, 8.50, TRUE),
('Instalacao assistida', 500.00, 12.00, TRUE);
