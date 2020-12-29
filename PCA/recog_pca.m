function recogRate = recog_pca(k)
    %displayEigenface(1);
    pc_train = {};
    %V=[];
    %M=[];
    %D_total=[];
    %Output_total=[];
    output=[];
    for j=1:65
       for i=1:k
            P = fullfile('C:\Users\markk\Downloads\Homework2\Homework2\PIE_Nolight\PIE_Nolight\',sprintf('%d',j),sprintf('%d.bmp',i));
            %imshow(P);
            d = imread(P);
            [d1 d2]=size(d);
            output=[output d(:)];
       end
       %Output_total=[Output_total output];
       %output=double(output);
       %m2=mean(output,2);
       %m3=mean(output,2);
       %output = output - m3;
       %G = output'*output;
       %[u D] = eig(G);
       %v= output*u; %convert it back to the real eigenvector set
       %v = v/norm(v);
       %D=abs(D);
       %D_diag = diag(D);
       %D_total = [D_total D_diag]; 
       %V=[V v];
    end
       %Output_total=[Output_total output];
       output=double(output);
       m2=mean(output,2);
       %m3=mean(output,2);
       output = output - m2;
       G = output'*output;
       [u D] = eig(G);
       v= output*u; %convert it back to the real eigenvector set
       v = v/norm(v);
       D=abs(D);
       D_diag = diag(D);
       %D_total = [D_total D_diag]; 
       %V=[V v];
        D_diag = D_diag(:);
        %Output_total=double(Output_total);
        %m2=mean(Output_total,2);
        %m2=mean(m2);
        %disp(size(V(:,1)));
        for i=1:(65*k)-1
           for j=1:(65*k)-1
                if D_diag(j) < D_diag(j+1)
                    tmp = v(:,j);
                    v(:,j) = v(:,j+1);
                    v(:,j+1) = tmp;
                end    
           end
        end    
        D_diag = sortrows(D_diag,1,'descend'); 
        %disp(D_total);
        %% store all projecQon coefficients in a 2 level cell array pc_train{}{}
        b=1;
        c=1;
        for i=1:65*k
           pc= v'*(output(:,i) - m2);
           pc_train{b}{c}=pc;  
           if c <= k
              c=c+1;
           end
           if c > k
              c=1;
              b=b+1;
           end
        end
        %% Test…
        classLabel = [];
        x=1;
        correctanswer_count=0;
        for i=1:65
            for y=k+1:21
                 P = fullfile('C:\Users\markk\Downloads\Homework2\Homework2\PIE_Nolight\PIE_Nolight\',sprintf('%d',i),sprintf('%d.bmp',y));
                 %imshow(P);
                 d = imread(P);
                 d=d(:);
                 d=double(d);
                 pc_test= v'*(d - m2);
                 minDist=inf;
                 for c=1:65
                      for j=1:k
                          %disp(pc_train{c}{j});
                          Dis=norm(pc_test- pc_train{c}{j});
                          if Dis<minDist
                              minDist=Dis;
                              classLabel(x)=c;                  
                          end
                      end
                  end
                  if classLabel(x)==i
                      correctanswer_count = correctanswer_count + 1;
                  end    
                  x=x+1; 
             end
        end
        %disp(classLabel);
        recogRate = correctanswer_count/(65*(21-k));
        fprintf('Recognition rate = %0.2f%%\n',recogRate*100);

end

