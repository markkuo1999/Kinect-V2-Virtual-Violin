function displayEigenface(pid)
    output=[];
    for i=1:21
        P = fullfile('C:\Users\markk\Downloads\Homework2\Homework2\PIE_Nolight\PIE_Nolight\',sprintf('%d',pid),sprintf('%d.bmp',i));
        %imshow(P);
        d = imread(P);
        [d1 d2]=size(d);
        output=[output d(:)];
    end
    output=double(output);
    %[V D m]=pca(output);
    %disp(output);
    m = mean(output);
    m = mean(m);
    disp(m);
    m2=mean(output,2);
    %figure(2);
    %disp(m2);
    output = output - m2;
    G = output'*output;
    [u D] = eig(G);
    v= output*u; %convert it back to the real eigenvector set
    v = v/norm(v);
    D=abs(D);
    D_diag = diag(D);
    for j=1:20
        if D_diag(j) < D_diag(j+1)
              tmp = v(:,j);
              v(:,j) = v(:,j+1);
              v(:,j+1) = tmp;
         end     
    end 
    D_diag = sortrows(D_diag,1,'descend'); 
    %disp(D_diag);
    subplot(6,4,1);
    im=reshape(m2,d1,d2);
    imagesc(im); colormap gray;
    title('mean');
    for i=1:21
        subplot(6,4,i+1);
        im=reshape(v(:,i),d1,d2);
        imagesc(im); colormap gray;
        title(sprintf('eigenface %d',i));
    end
 
end
