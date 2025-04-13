import { api } from './api';

// 响应接口定义
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

interface ExamResponse {
  id: number;
  title: string;
  description?: string;
  questions: any[];
  totalScore?: number;
  deadline?: string;
  isSubmitted?: boolean;
}

interface ExamSubmissionResponse {
  data: {
    score: number;
    totalScore: number;
    correctCount: number;
    questionCount: number;
    completionTime: number;
    submittedAt: string;
    answers: Array<{
      questionId: number;
      answer: string;
      isCorrect: boolean;
      score: number;
    }>;
  };
}

// 试卷服务
export const examService = {
  // 获取所有试卷（管理员）
  async getExams(): Promise<ApiResponse<ExamResponse[]>> {
    try {
      const response = await api.get<ApiResponse<ExamResponse[]>>('/exams');
      console.log('获取试卷原始响应:', response);

      // 如果返回空数组，创建一个空数组作为默认值
      if (!response) {
        console.warn('试卷响应为空');
        return { success: true, data: [] };
      }

      // 处理不同的响应格式
      if (response.success && Array.isArray(response.data)) {
        // 标准格式，直接返回
        return response;
      } else if (Array.isArray(response)) {
        // 如果响应本身就是数组，将其包装为标准格式
        return { success: true, data: response };
      } else if (response.data === null || response.data === undefined) {
        // 如果 data 字段为空，返回空数组
        console.warn('试卷响应的 data 字段为空');
        return { success: true, data: [] };
      }

      // 默认情况，直接返回原始响应
      return response as ApiResponse<ExamResponse[]>;
    } catch (error) {
      console.error('获取试卷列表失败:', error);
      // 返回空数组作为默认值，避免前端报错
      return { success: false, data: [] };
    }
  },

  // 获取学生的试卷
  async getStudentExams(): Promise<ExamResponse[]> {
    try {
      const response = await api.get<ExamResponse[]>('/exams');
      return Array.isArray(response) ? response : [];
    } catch (error) {
      console.error('获取学生试卷列表失败:', error);
      throw error;
    }
  },

  // 获取试卷详情
  async getExamById(id: number): Promise<ExamResponse> {
    try {
      return await api.get<ExamResponse>(`/exams/${id}`);
    } catch (error) {
      console.error(`获取试卷 ${id} 详情失败:`, error);
      throw error;
    }
  },

  // 创建试卷
  async createExam(examData: any) {
    try {
      const response = await api.post('/exams', examData);
      console.log('创建试卷响应:', response);
      return response;
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
  async getStudents(): Promise<ApiResponse<any[]>> {
    try {
      console.log('获取学生列表...');
      const response = await api.get<ApiResponse<any[]>>('/users/students');
      console.log('获取学生列表响应:', response);

      // 处理不同的响应格式
      if (response && response.success && Array.isArray(response.data)) {
        // 标准格式，直接返回
        return response;
      } else if (Array.isArray(response)) {
        // 如果响应本身就是数组，将其包装为标准格式
        return { success: true, data: response };
      } else if (!response || !response.data) {
        // 如果没有数据，返回空数组
        console.warn('学生列表为空');
        return { success: true, data: [] };
      }

      // 默认情况，直接返回原始响应
      return response as ApiResponse<any[]>;
    } catch (error) {
      console.error('获取学生列表失败:', error);
      throw error; // 抛出错误，让调用者处理
    }
  },

  // 分配试卷给学生
  async assignExam(examId: number, studentIds: number[]) {
    try {
      console.log(`准备分配试卷 ${examId} 给学生:`, studentIds);
      const response = await api.post(`/exams/${examId}/assign`, { studentIds });
      console.log(`分配试卷 ${examId} 响应:`, response);
      return response;
    } catch (error) {
      console.error(`分配试卷 ${examId} 失败:`, error);
      throw error; // 抛出错误，让调用者处理
    }
  },

  // 提交试卷
  async submitExam(examId: number, submission: any): Promise<ExamSubmissionResponse> {
    try {
      return await api.post<ExamSubmissionResponse>(`/exams/${examId}/submit`, submission);
    } catch (error) {
      console.error(`提交试卷 ${examId} 失败:`, error);
      throw error;
    }
  },

  // 获取试卷统计信息
  async getExamStatistics(examId: number) {
    try {
      console.log(`准备获取试卷 ${examId} 的统计信息`);
      const response = await api.get(`/exams/${examId}/statistics`);
      console.log(`获取试卷 ${examId} 统计信息响应:`, response);
      return response;
    } catch (error) {
      console.error(`获取试卷 ${examId} 统计信息失败:`, error);
      throw error; // 抛出错误，让调用者处理
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
