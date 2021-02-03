# BddDemo

[![license](https://img.shields.io/badge/license-Unlicense-blue.svg)](https://github.com/DejanMilicic/BddDemo/blob/main/LICENSE.md)
[![main](https://github.com/DejanMilicic/BddDemo/workflows/main/badge.svg)](https://github.com/DejanMilicic/BddDemo/blob/main/.github/workflows/main.yml)

----

To get Google "client_id":

1. https://developers.google.com/oauthplayground/
2. Find "Google OAuth2 API v2" and expand
3. Click on "https://www.googleapis.com/auth/userinfo.email" to select it
4. Click on "Authorize APIs" button
5. You will be presented with a choice of a Google account. Select one to proceed
6. Click on "Exchange authorization code for tokens" button
7. You will get URL that contains "client_id" as a query parameter
8. Below it, JSON contains "id_token" that you can use as a Bearer token