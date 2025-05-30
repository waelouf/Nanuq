module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: [
    'plugin:vue/vue3-essential',
    '@vue/airbnb',
  ],
  parserOptions: {
    parser: '@babel/eslint-parser',
  },
  rules: {
    'no-console': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
    'no-debugger': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
    'linebreak-style': 0,
    'operator-linebreak': 0,
    'import/no-extraneous-dependencies': 0,
    'vuejs-accessibility/click-events-have-key-events': 0,
    'max-len': 0,
    'vuejs-accessibility/anchor-has-content': 0,
    'no-shadow': 0,
    'vue/comment-directive': 0,
    'import/extensions': 0,
    'import/no-unresolved': 'off', // Disable import/no-unresolved errors
    requireConfigFile: 0,
    'vue/html-indent': 0,
    'vue/html-closing-bracket-spacing': 0,
    'vue/html-self-closing': 0,
  },
};
