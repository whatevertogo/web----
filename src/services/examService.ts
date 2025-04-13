import { api } from './api';

// 试卷服务
export const examService = {
  // 获取所有试卷（管理员）
  async getExams() {
    try {
      return await api.get('/exams');
    } catch (error) {
      console.error('获取试卷列表失败:', error);
      throw error;
    }
  },
  
  // 获取学生的试卷
  async getStudentExams() {
    try {
      return await api.get('/exams');
    } catch (error) {
      console.error('获取学生试卷列表失败:', error);
      throw error;
    }
  },
  
  // 获取试卷详情
  async getExamById(id: number) {
    try {
      return await api.get(`/exams/${id}`);
    } catch (error) {
      console.error(`获取试卷 ${id} 详情失败:`, error);
      throw error;
    }
  },
  
  // 创建试卷
  async createExam(examData: any) {
    try {
      return await api.post('/exams', examData);
    } catch (error) {
      console.error('创建试卷失败:', error);
      throw error;
    }
  },
  
  // 更新试卷
  async updateExam(examData: any) {
    try {
      return await api.put(`/exams/${examData.id}`, examData);
    } catch (error) {
      console.error(`更新试卷 ${examData.id} 失败:`, error);
      throw error;
    }
  },
  
  // 删除试卷
  async deleteExam(id: number) {
    try {
      return await api.delete(`/exams/${id}`);
    } catch (error) {
      console.error(`删除试卷 ${id} 失败:`, error);
      throw error;
    }
  },
  
  // 获取学生列表
  async getStudents() {
    try {
      return await api.get('/users/students');
    } catch (error) {
      console.error('获取学生列表失败:', error);
      throw error;
    }
  },
  
  // 分配试卷给学生
  async assignExam(examId: number, studentIds: number[]) {
    try {
      return await api.post(`/exams/${examId}/assign`, { studentIds });
    } catch (error) {
      console.error(`分配试卷 ${examId} 失败:`, error);
      throw error;
    }
  },
  
  // 提交试卷
  async submitExam(examId: number, submission: any) {
    try {
      return await api.post(`/exams/${examId}/submit`, submission);
    } catch (error) {
      console.error(`提交试卷 ${examId} 失败:`, error);
      throw error;
    }
  },
  
  // 获取试卷统计信息
  async getExamStatistics(examId: number) {
    try {
      return await api.get(`/exams/${examId}/statistics`);
    } catch (error) {
      console.error(`获取试卷 ${examId} 统计信息失败:`, error);
      throw error;
    }
  },
  
  // 获取试卷结果
  async getExamResults(examId: number, studentId?: number) {
    try {
      const url = studentId 
        ? `/exams/${examId}/results?studentId=${studentId}` 
        : `/exams/${examId}/results`;
      return await api.get(url);
    } catch (error) {
      console.error(`获取试卷 ${examId} 结果失败:`, error);
      throw error;
    }
  },
  
  // 导出试卷结果
  async exportExamResults(examId: number) {
    try {
      return await api.get(`/exams/${examId}/export`, {
        responseType: 'blob'
      });
    } catch (error) {
      console.error(`导出试卷 ${examId} 结果失败:`, error);
      throw error;
    }
  }
};
