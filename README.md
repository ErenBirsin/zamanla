# Zamanla

[![CI](https://github.com/ErenBirsin/zamanla/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/ErenBirsin/zamanla/actions/workflows/ci.yml)

Berber, kuaför ve güzellik salonları için çok kiracılı (multi-tenant) online randevu uygulaması.

> Geliştirme aşamasında. Ayrıntılı README, MVP tamamlandığında eklenecek.

## Teknolojiler

- **Backend:** .NET 10, ASP.NET Core Web API, Clean Architecture, EF Core, SQL Server
- **Frontend:** React 19, TypeScript, Vite, Tailwind CSS

## Proje yapısı

```text
src/
├── Zamanla.Domain           Entity'ler, value object'ler, iş kuralları
├── Zamanla.Application      Use case'ler (CQRS), validasyon
├── Zamanla.Infrastructure   EF Core, kimlik doğrulama, dış servisler
└── Zamanla.WebAPI           HTTP uç noktaları
```
