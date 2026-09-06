--
-- PostgreSQL database dump
-- POT E2E baseline: data-only dump of Site, User, and UserRole rows.
-- Regenerate only when canonical identities, role assignments, or non-nullable
-- columns without defaults change. When regenerating, strip non-standard psql
-- metacommands such as \restrict / \unrestrict.
--
-- NOTE: e2e_pwchange is a canonical seed user (Viewer role) whose
-- password is E2E_pwchange-password. It exists so the password-change
-- E2E test can rotate its own credentials without invalidating the shared admin.
--
-- NOTE: e2e_quickactions is a canonical seed user (Admin role) on its OWN site,
-- dedicated to the dashboard quick-actions suite so its whole-site renew/accrue
-- actions never touch the shared E2E site's data.
--


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
2	\N	4e0cdf24-4b98-4f5e-9953-6c966d3149d1	1788694945122	e2e_quickactions's Site
\.


--
-- Data for Name: User; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."User" ("Id", "Username", "Email", "DisplayName", "PasswordHash", "SiteId", "RowId", "Etag", "LastLoggedInUtc", "Status", "TokenVersion") FROM stdin;
1	e2e_admin	e2e_admin@local.test	e2e_admin	AQAAAAIAAYagAAAAEHAoGUVEdsKyglWASVe6lcqt98vguTtcyYYwmo4r5s44PLirZqJql2Se7Xh2N+jwSw==	1	58d1a6b2-bf25-40c1-b3d6-656ccf4a68b5	1788694457482	2026-09-06 11:34:17.24962+00	Enabled	2
2	e2e_viewer	e2e_viewer@local.test	e2e_viewer	AQAAAAIAAYagAAAAEOBTcdtOuFxRDyTYXN3sBc3+af6UvwSg/Vc3ZlT3l8VvZ9Wx4RuAQqeTytZrbRMYNQ==	1	e67c3105-7076-4f56-b373-01a8d0991295	1778896996815	2026-05-16 02:03:16.81478+00	Enabled	1
3	e2e_pwchange	e2e_pwchange@local.test	e2e_pwchange	AQAAAAIAAYagAAAAEI7Z1FxyDe4HTsQ7jZlNmHQ9PBZy2oe6hUvQ4AjGJA/iaBTlXBPgFi3jvWz1sTfmkg==	1	b72fd8d2-103a-422b-a8b8-cf68d8e6e79b	1787836078355	2026-08-27 13:06:36.903559+00	Enabled	1
4	e2e_quickactions	e2e_quickactions@local.test	e2e_quickactions	AQAAAAIAAYagAAAAEDr7V5W0KqqLph5UHjq4iLdY8W9b4RWbiMkX1KfYoLrUoUUi2Sbxn52c6EtqKgHM8A==	2	fa468896-6df2-4352-83f5-66995d8007f2	1788695165109	2026-09-06 11:44:55.407633+00	Enabled	1
\.


--
-- Data for Name: UserRole; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."UserRole" ("RolesId", "UsersId") FROM stdin;
1	1
2	2
2	3
1	4
\.


--
-- Name: Site_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Site_Id_seq"', 2, true);


--
-- Name: User_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."User_Id_seq"', 4, true);


--
-- PostgreSQL database dump complete
--

