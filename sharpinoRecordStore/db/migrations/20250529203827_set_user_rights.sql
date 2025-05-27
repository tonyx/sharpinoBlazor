-- migrate:up
GRANT ALL ON TABLE public.aggregate_events_01_user TO safe;
GRANT ALL ON SEQUENCE public.aggregate_events_01_user_id_seq to safe;
GRANT ALL ON TABLE public.events_01_user to safe;
GRANT ALL ON TABLE public.snapshots_01_user to safe;
GRANT ALL ON SEQUENCE public.snapshots_01_user_id_seq to safe;
          
GRANT ALL ON TABLE public.aggregate_events_01_item TO safe;
GRANT ALL ON SEQUENCE public.aggregate_events_01_item_id_seq to safe;
GRANT ALL ON TABLE public.events_01_item to safe;
GRANT ALL ON TABLE public.snapshots_01_item to safe;
GRANT ALL ON SEQUENCE public.snapshots_01_item_id_seq to safe;

-- migrate:down

