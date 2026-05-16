--
-- PostgreSQL database dump
--

\restrict CuSGeiIi7sHAucfga3SgKg4xkzdVNXDovMoQSdbKZR646kOwgIZlBxiUyL4qgyq

-- Dumped from database version 13.22 (Debian 13.22-1.pgdg13+1)
-- Dumped by pg_dump version 13.22 (Debian 13.22-1.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Data for Name: Site; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Site" ("Id", "Description", "RowId", "Etag", "Name") FROM stdin;
1	A Site for E2E testing	8faab70c-3aad-42d7-8a78-4744c0596e22	1778889518096	POT E2E Site
\.


--
-- Data for Name: User; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."User" ("Id", "Username", "Email", "DisplayName", "PasswordHash", "SiteId", "RowId", "Etag", "LastLoggedInUtc", "Status", "TokenVersion") FROM stdin;
1	e2e_admin	e2e_admin@local.test	e2e_admin	AQAAAAIAAYagAAAAEHAoGUVEdsKyglWASVe6lcqt98vguTtcyYYwmo4r5s44PLirZqJql2Se7Xh2N+jwSw==	1	58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5	1778896666691	2026-05-16 01:57:46.690278+00	Enabled	2
2	e2e_viewer	e2e_viewer@local.test	e2e_viewer	AQAAAAIAAYagAAAAEOBTcdtOuFxRDyTYXN3sBc3+af6UvwSg/Vc3ZlT3l8VvZ9Wx4RuAQqeTytZrbRMYNQ==	1	e67c3105-7076-4f56-b373-01a8d0991295	1778896996815	2026-05-16 02:03:16.81478+00	Enabled	1
\.


--
-- Data for Name: UserRole; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."UserRole" ("RolesId", "UsersId") FROM stdin;
1	1
2	2
\.


--
-- Name: Site_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Site_Id_seq"', 1, true);


--
-- Name: User_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."User_Id_seq"', 2, true);


--
-- PostgreSQL database dump complete
--

\unrestrict CuSGeiIi7sHAucfga3SgKg4xkzdVNXDovMoQSdbKZR646kOwgIZlBxiUyL4qgyq

