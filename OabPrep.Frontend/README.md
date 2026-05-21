# OAB Prep — Frontend

Interface web para integração com o backend .NET 8 da plataforma de estudos OAB.

---

## Estrutura do projeto

```
oab-prep/
├── index.html                          ← Entry point SPA
├── src/
│   ├── styles/
│   │   └── design-system.css           ← Design system compartilhado
│   ├── services/
│   │   └── api.js                      ← Todos os endpoints do backend
│   ├── store/
│   │   └── auth.js                     ← Tokens JWT + usuário logado
│   ├── utils/
│   │   └── router.js                   ← Roteador hash-based (#/rota)
│   ├── components/
│   │   ├── layout/
│   │   │   └── AppShell.js             ← Sidebar + header (páginas autenticadas)
│   │   ├── ui/
│   │   │   └── Toast.js                ← Notificações globais
│   │   └── chat/                       ← (expandir: ChatDrawer, etc.)
│   └── pages/
│       ├── auth/
│       │   ├── LoginPage.js            ← FE-03 (BE-04, BE-26)
│       │   ├── RegisterPage.js         ← FE-02 (BE-02)
│       │   ├── ForgotPasswordPage.js   ← FE-04 (BE-05)
│       │   ├── ResetPasswordPage.js    ← FE-04 (BE-05)
│       │   └── ConfirmEmailPage.js     ← (BE-03)
│       ├── HomePage.js                 ← FE-06 (BE-15, BE-14)
│       ├── study/
│       │   ├── SelectAreaPage.js       ← FE-07 (BE-10, BE-07)
│       │   ├── SessionPage.js          ← FE-08/09/10/11 (BE-11,12,13,21,22)
│       │   ├── ResultPage.js           ← FE-12 (BE-12)
│       │   └── HistoryPage.js          ← (BE-15)
│       ├── performance/
│       │   ├── PerformancePage.js      ← FE-14 (BE-15)
│       │   └── AreaDetailPage.js       ← FE-15 (BE-16)
│       ├── profile/
│       │   ├── ProfilePage.js          ← FE-05 (BE-17, BE-18)
│       │   └── PrivacyPage.js          ← FE-22 (BE-17, BE-18)
│       └── admin/
│           ├── QuestionsPage.js        ← FE-16 (BE-08, BE-09)
│           ├── UsersPage.js            ← FE-17 (BE-19)
│           └── ReportsPage.js          ← FE-18 (BE-20)
```

---

## Como rodar

### Pré-requisito
Qualquer servidor HTTP estático (o projeto usa ES Modules nativos do browser).

```bash
# Opção 1 — VS Code Live Server (recomendado)
# Abra o projeto e clique em "Go Live"

# Opção 2 — npx
npx serve .

# Opção 3 — Python
python3 -m http.server 3000
```

### Configurar a URL do backend
Edite `index.html`:
```js
window.ENV = {
  API_URL: 'http://localhost:5000/api/v1',  // ← URL do backend .NET
};
```

---

## Mapeamento de rotas

| Rota                  | Página               | Endpoints chamados               |
|-----------------------|----------------------|----------------------------------|
| `#/login`             | LoginPage            | POST /auth/login                 |
| `#/register`          | RegisterPage         | POST /auth/register              |
| `#/forgot-password`   | ForgotPasswordPage   | POST /auth/forgot-password       |
| `#/reset-password`    | ResetPasswordPage    | POST /auth/reset-password        |
| `#/`                  | HomePage             | GET /users/me/performance        |
| `#/select-area`       | SelectAreaPage       | GET /law-areas, POST /sessions   |
| `#/session/:id`       | SessionPage          | GET/POST /sessions/:id/...       |
| `#/result/:id`        | ResultPage           | POST /sessions/:id/finish        |
| `#/performance`       | PerformancePage      | GET /users/me/performance        |
| `#/performance/:area` | AreaDetailPage       | GET /users/me/performance/areas/:id |
| `#/profile`           | ProfilePage          | GET/PUT /users/me                |
| `#/privacy`           | PrivacyPage          | POST /users/me/data-export       |
| `#/admin/questions`   | QuestionsPage        | GET/POST/PUT /admin/questions    |
| `#/admin/users`       | UsersPage            | GET/PATCH /admin/users           |
| `#/admin/reports`     | ReportsPage          | GET /admin/reports/summary       |

---

## Autenticação

- Tokens armazenados em `localStorage` (produção React Native: `expo-secure-store`)
- Refresh automático em respostas 401 via interceptor em `api.js`
- Guard de rota em `router.js` redireciona para `/login` se sem token
- Rotas `/admin/*` exigem `role = Admin`

---

## Como adicionar uma nova página

1. Crie `src/pages/SuaPagina.js` com classe `export default`:
```js
import { AppShell } from '../components/layout/AppShell.js';

export default class SuaPagina {
  constructor(container, params) {
    this.container = container;
  }

  async render() {
    const shell = new AppShell(this.container, { title: 'Título', activeNav: 'id-nav' });
    shell.render(`<p>Conteúdo</p>`);
  }
}
```

2. Registre em `src/utils/router.js`:
```js
'/sua-rota': () => import('../pages/SuaPagina.js'),
```

---

## Integração com React Native (FE-01)

A camada de serviços (`src/services/api.js`) é compatível com React Native:
- Substitua `localStorage` em `store/auth.js` por `expo-secure-store`
- Substitua `window.location.hash` em `router.js` por `React Navigation`
- Mantenha `api.js` sem alterações — mesmos contratos de API

---

## Variáveis de ambiente

| Variável           | Descrição                        | Padrão                         |
|--------------------|----------------------------------|--------------------------------|
| `ENV.API_URL`      | Base URL do backend .NET 8       | `http://localhost:5000/api/v1` |

Para produção, injete via CI/CD no `index.html` antes do deploy.
