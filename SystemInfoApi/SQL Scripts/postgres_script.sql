drop table if exists "cpu_metrics";
create table "cpu_metrics"(
    "id" bigint generated always as identity primary key not null,
    cpu varchar(8) not null,
    "user" double precision default 0,
    "nice" double precision default 0,
    "system" double precision default 0,
    "idle" double precision default 0,
    "irq" double precision default 0,
    "softirq" double precision default 0,
    "steal" double precision default 0,
    "guest" double precision default 0,
    "guest_nice" double precision default 0,
    "current_stamp" timestamp default now()
);

create index cpu_metrics_current_stamp_idx on "cpu_metrics"(current_stamp);

drop table if exists "memory_metrics";
create table "memory_metrics"(
     "id" bigint generated always as identity primary key not null,
     "mem_total" double precision default 0,
     "mem_used" double precision default 0,
     "mem_free" double precision default 0,
     "mem_shared" double precision default 0,
     "mem_buffer" double precision default 0,
     "mem_available" double precision default 0,
     "swap_total" double precision default 0,
     "swap_used" double precision default 0,
     "swap_free" double precision default 0,
     "current_stamp" timestamp default now()
);

create index memory_metrics_current_stamp_idx on "memory_metrics"(current_stamp);

drop table if exists "drive_metrics";
create table "drive_metrics"(
    "id" bigint generated always as identity primary key not null,
    "name" varchar(200) default '',
    "drive_format" varchar(200) default '',
    "drive_type" varchar(64) default '',
    "volume_label" varchar(64) default '',
    "is_ready" bool default true,
    "total_free_space" double precision default 0,
    "available_free_space" double precision default 0,
    "total_size" double precision default 0,
    "current_stamp" timestamp default now()
);
create index drive_metrics_current_stamp_idx on "drive_metrics"(current_stamp);