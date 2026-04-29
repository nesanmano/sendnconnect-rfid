-- ============================================================
-- RFID Card Management System — MySQL 8.0 DDL
-- 15-table horizontal SaaS schema with tenant isolation
-- ============================================================

CREATE DATABASE IF NOT EXISTS card_management
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE card_management;

-- 1. tenant_settings
CREATE TABLE IF NOT EXISTS tenant_settings (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    display_name    VARCHAR(256)    NULL,
    default_currency VARCHAR(8)     NULL DEFAULT 'USD',
    timezone        VARCHAR(64)     NULL DEFAULT 'UTC',
    max_cards_per_holder INT        NOT NULL DEFAULT 5,
    allow_negative_balance TINYINT(1) NOT NULL DEFAULT 0,
    settings_json   JSON            NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_tenant_settings_org (org_id)
) ENGINE=InnoDB;

-- 2. custom_field_definitions
CREATE TABLE IF NOT EXISTS custom_field_definitions (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    entity_type     VARCHAR(32)     NOT NULL COMMENT 'CardHolder, Card, Merchant',
    field_key       VARCHAR(128)    NOT NULL,
    display_name    VARCHAR(256)    NULL,
    field_type      VARCHAR(32)     NOT NULL COMMENT 'Text, Number, Date, Boolean, Email, Phone, Url, Select, MultiSelect',
    is_required     TINYINT(1)      NOT NULL DEFAULT 0,
    is_searchable   TINYINT(1)      NOT NULL DEFAULT 0,
    is_pii          TINYINT(1)      NOT NULL DEFAULT 0,
    display_order   INT             NOT NULL DEFAULT 0,
    options_json    JSON            NULL COMMENT 'Allowed values for Select/MultiSelect',
    default_value   VARCHAR(512)    NULL,
    validation_regex VARCHAR(512)   NULL,
    is_active       TINYINT(1)      NOT NULL DEFAULT 1,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_cfd_org_entity_key (org_id, entity_type, field_key)
) ENGINE=InnoDB;

-- 3. card_holders
CREATE TABLE IF NOT EXISTS card_holders (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    external_id     VARCHAR(256)    NULL,
    display_name    VARCHAR(256)    NULL,
    email           VARCHAR(256)    NULL,
    phone           VARCHAR(32)     NULL,
    is_active       TINYINT(1)      NOT NULL DEFAULT 1,
    custom_data     JSON            NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_ch_org_ext (org_id, external_id),
    INDEX ix_ch_org (org_id)
) ENGINE=InnoDB;

-- 4. card_types
CREATE TABLE IF NOT EXISTS card_types (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    name            VARCHAR(128)    NOT NULL,
    description     VARCHAR(512)    NULL,
    default_currency VARCHAR(8)     NULL DEFAULT 'USD',
    daily_spend_limit DECIMAL(18,4) NULL DEFAULT 0,
    monthly_spend_limit DECIMAL(18,4) NULL DEFAULT 0,
    max_balance     DECIMAL(18,4)   NULL DEFAULT 0,
    validity_days   INT             NULL DEFAULT 0,
    is_active       TINYINT(1)      NOT NULL DEFAULT 1,
    custom_data     JSON            NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_ct_org_name (org_id, name),
    INDEX ix_ct_org (org_id)
) ENGINE=InnoDB;

-- 5. cards
CREATE TABLE IF NOT EXISTS cards (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    holder_id       CHAR(36)        NULL,
    card_type_id    CHAR(36)        NULL,
    uid             VARCHAR(128)    NOT NULL,
    label           VARCHAR(256)    NULL,
    status          VARCHAR(16)     NOT NULL DEFAULT 'Active' COMMENT 'Active, Blocked, Wiped, Expired',
    block_reason    VARCHAR(512)    NULL,
    replaced_by_id  CHAR(36)        NULL,
    custom_data     JSON            NULL,
    expires_at      DATETIME(6)     NULL,
    registered_at   DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_cards_org_uid (org_id, uid),
    INDEX ix_cards_org (org_id),
    INDEX ix_cards_holder (holder_id),
    INDEX ix_cards_type (card_type_id),
    CONSTRAINT fk_cards_holder FOREIGN KEY (holder_id) REFERENCES card_holders(id) ON DELETE SET NULL,
    CONSTRAINT fk_cards_type FOREIGN KEY (card_type_id) REFERENCES card_types(id) ON DELETE SET NULL,
    CONSTRAINT fk_cards_replaced FOREIGN KEY (replaced_by_id) REFERENCES cards(id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- 6. wallets
CREATE TABLE IF NOT EXISTS wallets (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    card_id         CHAR(36)        NOT NULL,
    balance         DECIMAL(18,4)   NOT NULL DEFAULT 0,
    currency        VARCHAR(8)      NOT NULL DEFAULT 'USD',
    daily_spent     DECIMAL(18,4)   NOT NULL DEFAULT 0,
    daily_reset_at  DATETIME(6)     NULL,
    monthly_spent   DECIMAL(18,4)   NOT NULL DEFAULT 0,
    monthly_reset_at DATETIME(6)    NULL,
    version         INT             NOT NULL DEFAULT 1,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_wallets_org_card (org_id, card_id),
    INDEX ix_wallets_org (org_id),
    CONSTRAINT fk_wallets_card FOREIGN KEY (card_id) REFERENCES cards(id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- 7. merchants
CREATE TABLE IF NOT EXISTS merchants (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    name            VARCHAR(256)    NOT NULL,
    location        VARCHAR(512)    NULL,
    terminal_id     VARCHAR(128)    NULL,
    is_active       TINYINT(1)      NOT NULL DEFAULT 1,
    custom_data     JSON            NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_merchants_org_tid (org_id, terminal_id),
    INDEX ix_merchants_org (org_id)
) ENGINE=InnoDB;

-- 8. transactions
CREATE TABLE IF NOT EXISTS transactions (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    wallet_id       CHAR(36)        NOT NULL,
    merchant_id     CHAR(36)        NULL,
    type            VARCHAR(16)     NOT NULL COMMENT 'Load, Spend, Refund, Adjustment, TransferIn, TransferOut',
    status          VARCHAR(16)     NOT NULL DEFAULT 'Completed' COMMENT 'Completed, Disputed, Reversed',
    amount          DECIMAL(18,4)   NOT NULL,
    balance_after   DECIMAL(18,4)   NOT NULL,
    reference       VARCHAR(256)    NULL,
    idempotency_key VARCHAR(128)    NULL,
    related_txn_id  CHAR(36)        NULL,
    description     VARCHAR(512)    NULL,
    custom_data     JSON            NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_txn_org_idem (org_id, idempotency_key),
    INDEX ix_txn_org (org_id),
    INDEX ix_txn_wallet (wallet_id),
    INDEX ix_txn_merchant (merchant_id),
    INDEX ix_txn_created (created_at),
    CONSTRAINT fk_txn_wallet FOREIGN KEY (wallet_id) REFERENCES wallets(id) ON DELETE CASCADE,
    CONSTRAINT fk_txn_merchant FOREIGN KEY (merchant_id) REFERENCES merchants(id) ON DELETE SET NULL,
    CONSTRAINT fk_txn_related FOREIGN KEY (related_txn_id) REFERENCES transactions(id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- 9. audit_logs
CREATE TABLE IF NOT EXISTS audit_logs (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    entity_type     VARCHAR(64)     NOT NULL,
    entity_id       CHAR(36)        NOT NULL,
    action          VARCHAR(64)     NOT NULL,
    actor_id        VARCHAR(128)    NULL,
    ip_address      VARCHAR(45)     NULL,
    user_agent      VARCHAR(512)    NULL,
    changes_json    JSON            NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    INDEX ix_al_org_entity (org_id, entity_type, entity_id),
    INDEX ix_al_created (created_at)
) ENGINE=InnoDB;

-- 10. blacklisted_uids
CREATE TABLE IF NOT EXISTS blacklisted_uids (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    uid             VARCHAR(128)    NOT NULL,
    reason          VARCHAR(512)    NULL,
    blocked_by      VARCHAR(128)    NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_bl_org_uid (org_id, uid)
) ENGINE=InnoDB;

-- 11. batch_operations
CREATE TABLE IF NOT EXISTS batch_operations (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    type            VARCHAR(32)     NOT NULL COMMENT 'Import, Block, Unblock, Wipe, Expire',
    status          VARCHAR(16)     NOT NULL DEFAULT 'Pending' COMMENT 'Pending, Processing, Completed, Failed, Partial',
    total_count     INT             NOT NULL DEFAULT 0,
    success_count   INT             NOT NULL DEFAULT 0,
    failure_count   INT             NOT NULL DEFAULT 0,
    error_details   JSON            NULL,
    initiated_by    VARCHAR(128)    NULL,
    started_at      DATETIME(6)     NULL,
    completed_at    DATETIME(6)     NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    INDEX ix_bo_org (org_id)
) ENGINE=InnoDB;

-- 12. auto_topup_rules
CREATE TABLE IF NOT EXISTS auto_topup_rules (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    wallet_id       CHAR(36)        NOT NULL,
    threshold       DECIMAL(18,4)   NOT NULL DEFAULT 0,
    topup_amount    DECIMAL(18,4)   NOT NULL DEFAULT 0,
    max_daily_topups INT            NOT NULL DEFAULT 3,
    is_active       TINYINT(1)      NOT NULL DEFAULT 1,
    last_triggered  DATETIME(6)     NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UNIQUE KEY uq_atr_org_wallet (org_id, wallet_id),
    CONSTRAINT fk_atr_wallet FOREIGN KEY (wallet_id) REFERENCES wallets(id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- 13. webhook_subscriptions
CREATE TABLE IF NOT EXISTS webhook_subscriptions (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    event_type      VARCHAR(128)    NOT NULL,
    target_url      VARCHAR(1024)   NOT NULL,
    secret          VARCHAR(256)    NULL,
    is_active       TINYINT(1)      NOT NULL DEFAULT 1,
    last_triggered  DATETIME(6)     NULL,
    failure_count   INT             NOT NULL DEFAULT 0,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    INDEX ix_ws_org_event (org_id, event_type)
) ENGINE=InnoDB;

-- 14. notification_logs
CREATE TABLE IF NOT EXISTS notification_logs (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    holder_id       CHAR(36)        NULL,
    channel         VARCHAR(16)     NOT NULL COMMENT 'Email, Sms, Push, Webhook',
    event_type      VARCHAR(128)    NOT NULL,
    recipient       VARCHAR(256)    NULL,
    subject         VARCHAR(256)    NULL,
    body_preview    VARCHAR(1024)   NULL,
    status          VARCHAR(16)     NOT NULL DEFAULT 'Queued' COMMENT 'Queued, Sent, Delivered, Failed, Bounced',
    failure_reason  VARCHAR(1024)   NULL,
    external_id     VARCHAR(256)    NULL,
    sent_at         DATETIME(6)     NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    INDEX ix_nl_org (org_id),
    INDEX ix_nl_holder (holder_id),
    CONSTRAINT fk_nl_holder FOREIGN KEY (holder_id) REFERENCES card_holders(id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- 15. custom_field_index
CREATE TABLE IF NOT EXISTS custom_field_index (
    id              CHAR(36)        NOT NULL PRIMARY KEY,
    org_id          VARCHAR(128)    NOT NULL,
    entity_type     VARCHAR(32)     NOT NULL,
    entity_id       CHAR(36)        NOT NULL,
    field_key       VARCHAR(128)    NOT NULL,
    field_value     VARCHAR(512)    NULL,
    INDEX ix_cfi_lookup (org_id, entity_type, field_key, field_value)
) ENGINE=InnoDB;
