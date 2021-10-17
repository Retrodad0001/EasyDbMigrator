CREATE TABLE IF NOT EXISTS distributors (
    code        char(5) CONSTRAINT codekey PRIMARY KEY,
    title       varchar(40) NOT NULL,
    did         integer NOT NULL,
    date_prod   date
);