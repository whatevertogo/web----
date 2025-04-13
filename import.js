// 导入题目数据到数据库
const axios = require('axios');

async function importQuestions() {
  console.log('Importing questions to database...');
  
  try {
    // 导入 dist/importQuestions.cjs 中的数据
    require('./dist/importQuestions.cjs');
    console.log('Import completed!');
  } catch (error) {
    console.error('Error importing questions:', error.message);
  }
}

// 执行导入
importQuestions();
