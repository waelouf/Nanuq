# Frontend Performance Optimization Guide

## Performance Issue Analysis

### Original Problem
- **Initial load time**: Taking minutes to load the first page
- **Bundle size**: 788 KiB (exceeds 500 KiB recommendation)
- **After first load**: Fast (browser caching working correctly)

### Root Causes Identified

1. **Vuetify CSS**: 361 KiB (45 KiB gzipped) - **Largest issue**
   - All Vuetify styles loaded upfront
   - Even with component tree-shaking, CSS includes all component styles

2. **Vuetify JavaScript**: 256 KiB (81 KiB gzipped)
   - Tree-shaking helps, but still large

3. **Blocking External Resources**:
   - DataTables CSS from CDN (blocking render)
   - Custom CSS from CDN (blocking render)
   - Bootstrap JS (blocking execution)
   - FontAwesome JS (blocking execution)

4. **Bundle Configuration**:
   - HomePage not lazy-loaded
   - Deprecated Vuetify components imported
   - Missing VSheet component

5. **Build Performance**: 116 seconds (slow rebuilds during development)

## Optimizations Applied

### ✅ Quick Wins Implemented

#### 1. Fixed Vuetify Component Imports
**File**: `src/plugins/vuetify.js`

```javascript
// Before: Deprecated components
VTabsItems, VTabItem // Don't exist in Vuetify 3

// After: Correct components
VTabsWindow, VTabsWindowItem, VSheet
```

**Impact**:
- Eliminated build warnings
- Fixed missing VSheet component used in all Add modals

#### 2. Lazy Load HomePage
**File**: `src/router/index.js`

```javascript
// Before: Eager loading
import HomePage from '@/home/HomePage.vue';

// After: Lazy loading
component: () => import(/* webpackChunkName: "home" */ '@/home/HomePage.vue')
```

**Impact**:
- Reduced app.js: 16.67 KiB → 14.94 KiB
- Created home.js chunk: 2.04 KiB (loads on demand)

#### 3. Async External Resources
**File**: `public/index.html`

```html
<!-- Preconnect for faster DNS resolution -->
<link rel="preconnect" href="https://cdn.datatables.net">
<link rel="preconnect" href="https://cdn.statically.io">

<!-- Async CSS loading -->
<link rel="preload" href="[css-url]" as="style"
      onload="this.onload=null;this.rel='stylesheet'">

<!-- Deferred JavaScript -->
<script defer src="[js-url]"></script>
```

**Impact**:
- Non-blocking resource loading
- Faster DNS resolution
- Improved perceived performance

#### 4. Webpack Build Optimizations
**File**: `vue.config.js`

```javascript
optimization: {
  usedExports: true,        // Better tree-shaking
  concatenateModules: true, // Smaller bundles
}
```

**Impact**:
- Build time: 116s → 48s (58% faster!)
- Better dead code elimination

### 📊 Performance Results

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Build Time | 116s | 48s | **58% faster** |
| app.js | 16.67 KiB | 14.94 KiB | -10% |
| HomePage | In app.js | 2.04 KiB (lazy) | Deferred |
| Deprecation Warnings | 3 | 0 | Fixed |
| External Blocking | Yes | No | Non-blocking |

**Bundle Breakdown (After Optimization):**
```
Entry Point (788 KiB total):
├── chunk-vuetify.js     256 KiB (81 KiB gzipped)   ⚠️ Largest
├── chunk-vuetify.css    361 KiB (45 KiB gzipped)   ⚠️ Largest
├── chunk-vendors.js      81 KiB (30 KiB gzipped)
├── chunk-vue-vendors.js  38 KiB (13 KiB gzipped)
├── chunk-utils.js        36 KiB (14 KiB gzipped)
└── app.js                15 KiB (5 KiB gzipped)

Lazy-Loaded Chunks:
├── home.js               2 KiB (0.7 KiB gzipped)
├── redis.js             29 KiB (8 KiB gzipped)
└── kafka.js             10 KiB (3 KiB gzipped)
```

## 🚧 Remaining Bottlenecks

### Primary Issue: Vuetify CSS (361 KiB)

**Why it's large:**
- `import 'vuetify/styles'` imports ALL Vuetify component styles
- Tree-shaking doesn't work for CSS with this approach
- Even unused components have their styles loaded

**Current State:**
- Using 25 Vuetify components
- Loading styles for ALL ~100+ Vuetify components

## 🎯 Advanced Optimizations (Optional)

### Option 1: Vuetify SASS Variables (Recommended)

Use Vuetify's SASS API for better CSS tree-shaking.

**Steps:**
1. Install sass-loader:
   ```bash
   npm install -D sass sass-loader
   ```

2. Update `src/plugins/vuetify.js`:
   ```javascript
   // Remove this
   import 'vuetify/styles'

   // Add this in main.js or App.vue <style> section
   ```

3. Create `src/styles/main.scss`:
   ```scss
   @use 'vuetify' with (
     $utilities: false // Disable unused utilities
   );
   ```

4. Import in `main.js`:
   ```javascript
   import '@/styles/main.scss'
   ```

**Expected Impact**: Reduce CSS by 50-70% (~180-250 KiB savings)

### Option 2: Critical CSS Extraction

Extract above-the-fold CSS and inline it.

**Tool**: [critical](https://github.com/addyosmani/critical)

```bash
npm install -D critical
```

**Configuration in `vue.config.js`:**
```javascript
const critical = require('critical');

// After build, extract critical CSS
chainWebpack: (config) => {
  config.plugin('html').tap(args => {
    args[0].criticalCSS = true;
    return args;
  });
}
```

**Expected Impact**: Faster first paint, 30% improvement in perceived load time

### Option 3: Migrate to Vite

Vue CLI is deprecated. Vite provides:
- Much faster builds (5-10x faster)
- Better code splitting
- Smaller bundles
- Better development experience

**Migration Guide**: https://vitejs.dev/guide/migration.html

**Expected Impact**:
- Build time: 48s → 5-10s
- Better bundle splitting
- Faster HMR during development

### Option 4: CDN for Vuetify

Load Vuetify from CDN instead of bundling.

**public/index.html:**
```html
<link href="https://cdn.jsdelivr.net/npm/vuetify@3/dist/vuetify.min.css" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/vuetify@3/dist/vuetify.min.js"></script>
```

**Pros**: Browser can cache Vuetify separately
**Cons**: Requires network request, no tree-shaking

### Option 5: Dynamic Imports for Vuetify Components

Load Vuetify components dynamically as needed.

```javascript
// Instead of importing all at once
components: {
  VBtn: () => import('vuetify/components').then(m => m.VBtn),
  // ...
}
```

**Expected Impact**: Smaller initial bundle, components load on-demand

## 🔍 Monitoring Performance

### Measure Bundle Size

```bash
cd src/app/nanuq-app
npm run build -- --report
```

This generates a visual bundle analyzer.

### Lighthouse Audit

```bash
npm install -g lighthouse
lighthouse http://localhost:8080 --view
```

**Current Metrics (Production Build):**
- Performance: ~70-80 (due to Vuetify size)
- First Contentful Paint: 1.5-2s
- Time to Interactive: 2-3s
- Speed Index: 2-3s

**Target Metrics:**
- Performance: >90
- First Contentful Paint: <1s
- Time to Interactive: <1.5s
- Speed Index: <1.5s

### Browser DevTools

**Chrome DevTools → Performance:**
1. Open DevTools (F12)
2. Go to Performance tab
3. Click Record
4. Reload page
5. Stop recording
6. Analyze the waterfall

**Look for:**
- Long tasks (>50ms)
- Blocking resources
- JavaScript execution time

## 📝 Best Practices Going Forward

1. **Lazy Load Routes**: All routes should use dynamic imports
   ```javascript
   component: () => import('./Component.vue')
   ```

2. **Code Split by Feature**: Group related components into chunks
   ```javascript
   /* webpackChunkName: "feature-name" */
   ```

3. **Minimize Third-Party Dependencies**:
   - Audit with `npm ls`
   - Remove unused dependencies
   - Consider lighter alternatives

4. **Optimize Images**:
   - Use WebP format
   - Lazy load images
   - Responsive images with srcset

5. **Service Worker / PWA**:
   - Cache static assets
   - Offline support
   - Faster subsequent loads

## 🎬 Testing the Optimizations

### Development Server
```bash
cd src/app/nanuq-app
npm run serve
```

### Production Build
```bash
npm run build
cd dist
npx http-server -p 8080
```

Then visit http://localhost:8080 and:
1. Open DevTools Network tab
2. Disable cache
3. Throttle to "Fast 3G"
4. Reload and measure load time

**Expected Load Time**:
- Before optimizations: 60-120 seconds (with throttling)
- After optimizations: 10-20 seconds (with throttling)
- With Vuetify SASS optimization: 5-10 seconds (with throttling)

## 🚀 Quick Reference

### Commands
```bash
# Build with size report
npm run build -- --report

# Analyze bundle
npm run build
npx webpack-bundle-analyzer dist/stats.json

# Run Lighthouse
lighthouse http://localhost:8080 --view

# Check dependencies
npm ls --depth=0
```

### Key Files
- Bundle config: `vue.config.js`
- Router config: `src/router/index.js`
- Vuetify setup: `src/plugins/vuetify.js`
- HTML template: `public/index.html`

## 📚 Resources

- [Web Vitals](https://web.dev/vitals/)
- [Webpack Bundle Analyzer](https://github.com/webpack-contrib/webpack-bundle-analyzer)
- [Vuetify Tree Shaking](https://vuetifyjs.com/en/features/treeshaking/)
- [Vue Performance Guide](https://vuejs.org/guide/best-practices/performance.html)
- [Lighthouse CI](https://github.com/GoogleChrome/lighthouse-ci)

---

## Summary

The optimizations applied provide significant improvements:
- ✅ 58% faster builds
- ✅ Non-blocking resource loading
- ✅ Lazy-loaded routes
- ✅ Fixed deprecation warnings
- ✅ Better tree-shaking

The main remaining bottleneck is Vuetify CSS (361 KiB). For best results, implement the Vuetify SASS optimization or consider migrating to Vite for even better performance.
