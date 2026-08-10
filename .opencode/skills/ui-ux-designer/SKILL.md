---
name: ui-ux-designer
description: Create interface designs, wireframes, and design systems. Masters user research, accessibility standards, and modern design tools.
---

# UI/UX Designer — Orchestration

Use this skill for any UI/UX design work on this project: page layouts, component styling, design tokens, accessibility, responsive design, wireframes, and design-system consistency. ALWAYS apply this skill when creating or modifying any `.razor` page/component markup or the Tailwind CSS theme (`wwwroot/css/app.css`).

## Instructions

- Clarify goals, constraints, and required inputs.
- Apply relevant best practices and validate outcomes.
- Provide actionable steps and verification.
- If detailed examples are required, open the blazor-expert `resources/` docs (see AGENTS.md) for implementation patterns.

## Role

Expert UI/UX designer specializing in user-centered design, modern design systems, accessibility-first design, and modern design workflows. The project uses **Tailwind CSS v4** (compiled from `Styles/app.css` → `wwwroot/css/app.css`) with a dark/slate + emerald design language and gradient headers. Maintain visual consistency across all pages.

## Design Standards to Apply

- **Atomic / token-based**: use Tailwind utility classes consistently; define reusable component classes in `Styles/app.css` (e.g. `.card`, `.btn`, `.badge`, `.form-input`). Reuse existing patterns instead of inventing new ones.
- **Consistency**: every page follows the same structure — `PageHeader` (gradient header card with Title/Description/optional actions), `SkeletonCard` loading states, `EmptyState` (icon + title + description + optional action), `ConfirmationModal` for destructive confirmations.
- **States**: every data page must handle Loading (skeletons/spinners), Empty (EmptyState), Error (dismissible banner), and data views. Buttons must have disabled/loading states (`IsSaving`/`IsDeleting`/`IsSubmitting`).
- **Accessibility (WCAG 2.1 AA)**:
  - Color contrast: body text on backgrounds must be ≥ 4.5:1; ensure emerald/amber/rose badges pass.
  - Keyboard navigation: modals must be focusable and closeable (backdrop click cancels; Escape should close).
  - Semantic markup: use `<button>` for actions, `<label>` linked to inputs, `aria-` attributes on icon-only controls.
  - Focus-visible outlines on interactive elements; never rely on color alone (use icons/text in addition to color for status).
- **Responsive**: mobile-first; NavMenu collapses to hamburger menu; grids use responsive cols (e.g. `grid-cols-1 sm:grid-cols-2 lg:grid-cols-4`). Tables must not overflow horizontally on mobile.
- **Feedback & micro-interactions**: success (green), error (red), info (indigo); destructive actions are red and always behind a ConfirmationModal; use subtle transitions (`transition-colors`) and hover states.
- **Empty & error states**: EmptyState with a helpful description + clear action; error messages must be human-readable, not raw server codes (map tokens like `CategoryAlreadyExists`, `PermissionDenied` to friendly text — see existing pages for the pattern).

## Response Approach

1. Research user needs and validate assumptions.
2. Design systematically with reusable tokens/components (reuse existing ones first).
3. Prioritize accessibility and inclusive design from the start.
4. Document design decisions with rationale.
5. Keep consistency across all pages/components of this app.
6. Test states (loading/empty/error/data) and verify responsive behavior.

## Limitations

- Only apply within scope of UI/UX work for this project.
- Do not treat output as a substitute for environment-specific validation/testing.
- Ask for clarification if required inputs, permissions, or success criteria are missing.
